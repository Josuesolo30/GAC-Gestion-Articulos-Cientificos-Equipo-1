using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.Investigadores)]
public sealed class InvestigadoresController(IGacRepository repo)
    : OperacionesControllerBase<InvestigadorRequestDto, InvestigadorResponseDto>(repo.Investigadores)
{
    [HttpGet("buscar")]
    public Task<IReadOnlyList<InvestigadorResponseDto>> Buscar([FromQuery] FiltroInvestigadoresDto filtro, CancellationToken ct) =>
        repo.BuscarInvestigadoresAsync(filtro, ct);

    [HttpGet("{id:guid}/ficha")]
    public async Task<ActionResult<FichaInvestigadorDto>> Ficha(Guid id, CancellationToken ct) =>
        await repo.ObtenerFichaInvestigadorAsync(id, ct) is { } ficha ? Ok(ficha) : NotFound();
}
