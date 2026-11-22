using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Catalogos;

public sealed class IndexModel(GestorCatalogos gestor) : PaginaGac
{
    public string Catalogo { get; private set; } = "";
    public string Titulo => GestorCatalogos.Nombres[Catalogo];
    public IReadOnlyList<ItemCatalogo> Registros { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string? catalogo, CancellationToken ct)
    {
        Catalogo = catalogo ?? "tipos-informe";
        if (!GestorCatalogos.Nombres.ContainsKey(Catalogo)) return NotFound();
        Registros = await gestor.ListarAsync(Catalogo, ct);
        return Page();
    }
}
