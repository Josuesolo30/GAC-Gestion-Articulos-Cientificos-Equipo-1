using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Catalogos;

public sealed class EliminarModel(GestorCatalogos gestor) : PaginaGac
{
    public string Catalogo { get; private set; } = "";
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = "";

    private async Task<bool> CargarAsync(string catalogo, Guid id, CancellationToken ct)
    {
        if (!GestorCatalogos.Nombres.ContainsKey(catalogo)) return false;
        if (await gestor.ObtenerAsync(catalogo, id, ct) is not { } item) return false;
        Catalogo = catalogo;
        Id = id;
        Nombre = item.Nombre ?? item.Edicion ?? "(sin nombre)";
        return true;
    }

    public async Task<IActionResult> OnGetAsync(string catalogo, Guid id, CancellationToken ct) =>
        await CargarAsync(catalogo, id, ct) ? Page() : NotFound();

    public async Task<IActionResult> OnPostAsync(string catalogo, Guid id, CancellationToken ct)
    {
        if (!await CargarAsync(catalogo, id, ct)) return NotFound();
        var r = await gestor.EliminarAsync(catalogo, id, ct);
        if (!r.Success) return await RevisarAsync(r) ?? Page();
        TempData["Aviso"] = "Registro eliminado.";
        return RedirectToPage("./Index", new { catalogo });
    }
}
