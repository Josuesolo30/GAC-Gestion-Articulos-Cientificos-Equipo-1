using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Investigadores;

public sealed class DetalleModel(IGacService gac) : PaginaGac
{
    public FichaInvestigadorDto Ficha { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        if (await gac.ObtenerFichaInvestigadorAsync(id, ct) is not { } f) return NotFound();
        Ficha = f;
        return Page();
    }
}
