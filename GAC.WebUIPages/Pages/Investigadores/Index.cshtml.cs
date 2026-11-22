using System.ComponentModel.DataAnnotations;
using GAC.Application.Interfaces;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WebUIPages.Pages.Investigadores;

public sealed class IndexModel(IGacService gac) : PaginaGac
{
    [BindProperty(SupportsGet = true), MaxLength(250)] public string? Texto { get; set; }
    [BindProperty(SupportsGet = true), RegularExpression("todos|nombre|centro|tema")] public string? Campo { get; set; }
    public IReadOnlyList<InvestigadorResponseDto> Resultados { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (ModelState.IsValid)
            Resultados = await gac.BuscarInvestigadoresAsync(new FiltroInvestigadoresDto { Texto = Texto, Campo = Campo }, ct);
    }
}
