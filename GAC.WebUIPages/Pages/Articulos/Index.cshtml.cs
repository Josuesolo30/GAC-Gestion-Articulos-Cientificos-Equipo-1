using System.ComponentModel.DataAnnotations;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Articulos;

public sealed class IndexModel(IGacService gac) : PaginaGac
{
    [BindProperty(SupportsGet = true), MaxLength(250)] public string? Texto { get; set; }
    [BindProperty(SupportsGet = true), RegularExpression("todos|titulo|autor|palabra")] public string? Campo { get; set; }
    [BindProperty(SupportsGet = true), Range(1, 3)] public int? Tipo { get; set; }
    [BindProperty(SupportsGet = true)] public bool? TieneCopia { get; set; }
    public IReadOnlyList<ResumenArticuloDto> Resultados { get; private set; } = [];
    public bool Filtrado => !string.IsNullOrWhiteSpace(Texto) || Tipo is not null || TieneCopia is not null;

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (ModelState.IsValid)
            Resultados = await gac.BuscarArticulosAsync(new FiltroArticulosDto
                { Texto = Texto, Campo = Campo, Tipo = Tipo, TieneCopia = TieneCopia }, ct);
    }
}
