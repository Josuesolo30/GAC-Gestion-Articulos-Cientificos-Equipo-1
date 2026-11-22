using GAC.Application.Constants;
using GAC.Domain.Entity.Authentication;
using GAC.Share.DTO.Request.Account;
using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.Cuentas)]
public sealed class CuentasController(UserManager<ApplicationUser> usuarios, SignInManager<ApplicationUser> acceso) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Entrar(LoginRequestDto solicitud)
    {
        var usuario = await usuarios.FindByEmailAsync(solicitud.Email.Trim());
        if (usuario is null || !(await acceso.CheckPasswordSignInAsync(usuario, solicitud.Password, lockoutOnFailure: true)).Succeeded)
            return Unauthorized(new GeneralResponseDto(false, "Correo o contraseña incorrectos."));

        var principal = await acceso.CreateUserPrincipalAsync(usuario);
        return SignIn(principal, IdentityConstants.BearerScheme);
    }
}
