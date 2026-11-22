using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class DetalleModel(IGacService gac) : PaginaGac
{
    public FichaArticuloDto Ficha { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        if (await gac.ObtenerFichaArticuloAsync(id, ct) is not { } ficha) return NotFound();
        Ficha = ficha;
        return Page();
    }
}
