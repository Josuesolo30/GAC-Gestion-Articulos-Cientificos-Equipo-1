using GAC.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAC.WebUIPages.Pages.Catalogos;

public sealed class EditarModel(GestorCatalogos gestor, IGacService gac) : PaginaGac
{
    public string Catalogo { get; private set; } = "";
    public Guid? Id { get; private set; }
    public string Titulo => GestorCatalogos.Nombres[Catalogo];
    [BindProperty] public DatosCatalogo Datos { get; set; } = new();
    public IReadOnlyList<SelectListItem> Temas { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Congresos { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Investigadores { get; private set; } = [];

    private async Task Opciones(CancellationToken ct)
    {
        if (Catalogo is "revistas" or "grupos-investigacion")
            Temas = (await gac.Temas.ListarAsync(ct)).Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToArray();
        if (Catalogo == "ediciones-congreso")
            Congresos = (await gac.Congresos.ListarAsync(ct)).Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToArray();
        if (Catalogo == "revistas")
            Investigadores = (await gac.Investigadores.ListarAsync(ct)).Select(x => new SelectListItem($"{x.Nombre} · {x.CentroTrabajo}", x.Id.ToString())).ToArray();
    }

    public async Task<IActionResult> OnGetAsync(string catalogo, Guid? id, CancellationToken ct)
    {
        if (!GestorCatalogos.Nombres.ContainsKey(catalogo)) return NotFound();
        Catalogo = catalogo;
        Id = id;
        if (id is Guid k)
        {
            if (await gestor.ObtenerAsync(catalogo, k, ct) is not { } f) return NotFound();
            Datos = f;
        }
        await Opciones(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string catalogo, Guid? id, CancellationToken ct)
    {
        if (!GestorCatalogos.Nombres.ContainsKey(catalogo)) return NotFound();
        Catalogo = catalogo;
        Id = id;
        if (ModelState.IsValid)
        {
            var resultado = await gestor.GuardarAsync(catalogo, id, Datos, ct);
            if (resultado.Success)
            {
                TempData["Aviso"] = id is null ? "Registro creado." : "Cambios guardados.";
                return RedirectToPage("./Index", new { catalogo });
            }
            if (await RevisarAsync(resultado) is { } respuesta) return respuesta;
        }
        await Opciones(ct);
        return Page();
    }
}
