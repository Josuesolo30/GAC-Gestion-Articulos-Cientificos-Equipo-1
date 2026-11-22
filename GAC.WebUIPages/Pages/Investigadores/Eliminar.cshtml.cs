using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Investigadores;

public sealed class EliminarModel(IGacService gac) : PaginaGac
{
    public InvestigadorResponseDto Investigador { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        if (await gac.Investigadores.ObtenerAsync(id, ct) is not { } i) return NotFound();
        Investigador = i;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        if (await gac.Investigadores.ObtenerAsync(id, ct) is not { } i) return NotFound();
        Investigador = i;
        var r = await gac.Investigadores.EliminarAsync(id, ct);
        if (!r.Success) return await RevisarAsync(r) ?? Page();
        TempData["Aviso"] = "Se eliminó el investigador.";
        return RedirectToPage("./Index");
    }
}
