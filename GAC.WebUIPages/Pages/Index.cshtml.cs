using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GAC.WebUIPages.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Articulos/Index");
}
