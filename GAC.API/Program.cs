using GAC.Application.Contracts;
using GAC.Domain.Entity.Authentication;
using GAC.Infrastructure.Data;
using GAC.Infrastructure.Repositories;
using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAC.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var prepararDemo = args.Contains("--seed-demo", StringComparer.Ordinal);
        var builder = WebApplication.CreateBuilder(args.Where(x => x != "--seed-demo").ToArray());
        var connection = builder.Configuration.GetConnectionString("GacDb")
            ?? throw new InvalidOperationException("Configura ConnectionStrings:GacDb para iniciar GAC.");

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
        builder.Services.AddScoped<IGacRepository, GacRepository>();
        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddEntityFrameworkStores<AppDbContext>().AddSignInManager();
        builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme, options => options.BearerTokenExpiration = TimeSpan.FromMinutes(30));
        builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        builder.Services.AddProblemDetails();
        builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errores = context.ModelState.Where(p => p.Value?.Errors.Count > 0).ToDictionary(
                    p => p.Key, p => p.Value!.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Revisa el formato de este campo." : e.ErrorMessage).ToArray());
                return new BadRequestObjectResult(new GeneralResponseDto(false, "Corrige los campos indicados.", Errors: errores, Estado: 400));
            };
        });
        builder.Services.AddOpenApi();

        var app = builder.Build();
        if (prepararDemo)
        {
            if (!app.Environment.IsDevelopment())
                throw new InvalidOperationException("Los datos de ejemplo solo se cargan en Development.");
            await using var scope = app.Services.CreateAsyncScope();
            await DatosDemo.PrepararAsync(scope.ServiceProvider, app.Configuration);
            return;
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.MapControllers();
        await app.RunAsync();
    }
}
