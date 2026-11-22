using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages;

public abstract class PaginaGac : PageModel
{
    protected async Task<IActionResult?> RevisarAsync(GeneralResponseDto resultado, string entrada = "Datos")
    {
        if (resultado.Success) return null;
        if (resultado.Estado == 401)
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Cuenta/Entrar");
        }
        if ((resultado.Errors?.Count ?? 0) == 0)
            ModelState.AddModelError("", resultado.Message ?? "No se pudo realizar la operación.");
        else
            foreach (var (campo, mensajes) in resultado.Errors!)
                foreach (var mensaje in mensajes)
                    ModelState.AddModelError(string.IsNullOrWhiteSpace(campo) ? "" : entrada + "." + campo.TrimStart('$', '.'), mensaje);
        return null;
    }

    public static string TipoArticulo(int tipo) => tipo switch
    {
        1 => "Informe técnico",
        2 => "Congreso",
        3 => "Revista",
        _ => "Modalidad desconocida"
    };
}
