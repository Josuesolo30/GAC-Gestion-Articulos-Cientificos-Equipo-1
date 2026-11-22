using GAC.Application.Interfaces;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class CambiarModalidadModel(IGacService gac) : EditorArticulo(gac)
{
    public ArticuloResponseDto Articulo { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var ficha = await Gac.Articulos.ObtenerAsync(id, ct);
        if (ficha is null) return NotFound();
        Articulo = ficha;
        Datos.TipoPublicacion = ficha.TipoPublicacion == 1 ? 2 : 1;
        await CargarAsync(false, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        var ficha = await Gac.Articulos.ObtenerAsync(id, ct);
        if (ficha is null) return NotFound();
        Articulo = ficha;
        if (ModelState.IsValid)
        {
            var resultado = await Gac.CambiarModalidadAsync(id, NuevoTipo(), ct);
            if (resultado.Success)
            {
                TempData["Aviso"] = "Modalidad actualizada. Los autores, palabras clave y copia se conservaron.";
                return RedirectToPage("./Detalle", new { id });
            }
            if (await RevisarAsync(resultado) is { } respuesta) return respuesta;
        }
        await CargarAsync(false, ct);
        return Page();
    }
}
