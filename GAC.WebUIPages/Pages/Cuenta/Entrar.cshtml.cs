using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using GAC.Application.Constants;
using GAC.Share.DTO.Request.Account;
using GAC.WebUIPages.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages.Cuenta;

[AllowAnonymous]
public sealed class EntrarModel(IHttpClientFactory clientes) : PageModel
{
    [BindProperty] public Entrada Datos { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true
        ? RedirectToPage("/Articulos/Index") : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        using var cliente = clientes.CreateClient(GacApiRoutes.ClientName);
        try
        {
            using var respuesta = await cliente.PostAsJsonAsync(GacApiRoutes.Login,
                new LoginRequestDto { Email = Datos.Email.Trim(), Password = Datos.Password }, ct);
            if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return Page();
            }
            if (!respuesta.IsSuccessStatusCode || await respuesta.Content.ReadFromJsonAsync<RespuestaAcceso>(ct) is not { } acceso
                || string.IsNullOrWhiteSpace(acceso.AccessToken) || acceso.ExpiresIn <= 0)
            {
                ModelState.AddModelError("", "No se pudo iniciar sesión. Comprueba que la API esté disponible.");
                return Page();
            }

            await HttpContext.Session.LoadAsync(ct);
            HttpContext.Session.Clear();
            HttpContext.Session.SetString(SesionGac.ClaveToken, acceso.AccessToken);
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, Datos.Email.Trim())], CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario,
                new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Min(1800, acceso.ExpiresIn)) });

            return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl) : RedirectToPage("/Articulos/Index");
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException)
        {
            ModelState.AddModelError("", "No se pudo conectar con la API de GAC. Iníciala e inténtalo otra vez.");
            return Page();
        }
    }

    public sealed class Entrada
    {
        [Required(ErrorMessage = "Indica el correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), MaxLength(254)]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Indica la contraseña."), DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    private sealed record RespuestaAcceso(string AccessToken, int ExpiresIn);
}
