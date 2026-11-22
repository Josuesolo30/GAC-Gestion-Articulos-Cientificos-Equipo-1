using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages.Cuenta;

public sealed class SalirModel : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.Session.LoadAsync(HttpContext.RequestAborted);
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Cuenta/Entrar");
    }
}
