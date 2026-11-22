using GAC.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class EditarModel(IGacService gac) : EditorArticulo(gac)
{
    public Guid? Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid? id, int? tipo, CancellationToken ct)
    {
        Id = id;
        if (id is Guid k)
        {
            var ficha = await Gac.ObtenerFichaArticuloAsync(k, ct);
            if (ficha is null) return NotFound();
            LlenarDesde(ficha);
        }
        else Datos.TipoPublicacion = tipo is >= 1 and <= 3 ? tipo.Value : 1;
        await CargarAsync(true, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id, CancellationToken ct)
    {
        // El identificador de la ruta manda; nunca se elige un registro a partir del formulario.
        Id = id;
        if (Id is Guid k)
        {
            var actual = await Gac.Articulos.ObtenerAsync(k, ct);
            if (actual is null) return NotFound();
            if (actual.TipoPublicacion != Datos.TipoPublicacion)
                ModelState.AddModelError("Datos.TipoPublicacion", "Usa la opción Cambiar modalidad en la ficha.");
        }
        if (ModelState.IsValid)
        {
            var resultado = await GuardarArticuloAsync(Id, ct);
            if (resultado.Success)
            {
                TempData["Aviso"] = Id is null ? "Artículo registrado." : "Cambios guardados.";
                return RedirectToPage("./Detalle", new { id = resultado.Id ?? Id });
            }
            if (await RevisarAsync(resultado) is { } respuesta) return respuesta;
        }
        await CargarAsync(true, ct);
        return Page();
    }
}
