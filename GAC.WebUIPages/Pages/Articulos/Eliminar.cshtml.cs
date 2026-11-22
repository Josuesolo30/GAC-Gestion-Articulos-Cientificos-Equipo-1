using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class EliminarModel(IGacService gac) : PaginaGac
{
    public ArticuloResponseDto Articulo { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        if (await gac.Articulos.ObtenerAsync(id, ct) is not { } a) return NotFound();
        Articulo = a;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        if (await gac.Articulos.ObtenerAsync(id, ct) is not { } a) return NotFound();
        Articulo = a;
        var r = await (a.TipoPublicacion switch
        {
            1 => gac.InformesTecnicos.EliminarAsync(id, ct),
            2 => gac.PublicacionesCongreso.EliminarAsync(id, ct),
            _ => gac.PublicacionesRevista.EliminarAsync(id, ct)
        });
        if (!r.Success) return await RevisarAsync(r) ?? Page();
        TempData["Aviso"] = "Se eliminó el artículo.";
        return RedirectToPage("./Index");
    }
}
