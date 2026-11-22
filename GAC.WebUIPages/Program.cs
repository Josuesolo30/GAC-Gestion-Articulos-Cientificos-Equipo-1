using GAC.Application.Constants;
using GAC.Application.Extensions;
using GAC.WebUIPages.Seguridad;
using GAC.WebUIPages.Pages.Catalogos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace GAC.WebUIPages;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<GestorCatalogos>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.Name = ".Gac.Sesion";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/Cuenta/Entrar";
            options.Cookie.Name = ".Gac.Acceso";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = false;
            options.Events.OnValidatePrincipal = async context =>
            {
                await context.HttpContext.Session.LoadAsync(context.HttpContext.RequestAborted);
                if (context.HttpContext.Session.GetString(SesionGac.ClaveToken) is null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync();
                }
            };
        });
        builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        var apiUrl = builder.Configuration[GacApiRoutes.ConfigurationKey]
            ?? throw new InvalidOperationException("Configura GacApi:BaseUrl para iniciar GAC.");
        builder.Services.AddGacApplication(new Uri(apiUrl));
        builder.Services.AddTransient<CredencialApiHandler>();
        builder.Services.AddHttpClient(GacApiRoutes.ClientName).AddHttpMessageHandler<CredencialApiHandler>();

        var app = builder.Build();
        app.UseExceptionHandler("/Error");
        if (!app.Environment.IsDevelopment()) app.UseHsts();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        app.Use(async (context, siguiente) =>
        {
            try { await siguiente(context); }
            catch (HttpRequestException error) when (error.StatusCode == System.Net.HttpStatusCode.Unauthorized && !context.Response.HasStarted)
            {
                context.Session.Clear();
                await context.SignOutAsync();
                context.Response.Redirect("/Cuenta/Entrar");
            }
        });
        app.MapStaticAssets().AllowAnonymous();
        app.MapRazorPages().WithStaticAssets();
        app.Run();
    }
}
