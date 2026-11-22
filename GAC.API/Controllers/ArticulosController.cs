using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.Articulos)]
public sealed class ArticulosController(IGacRepository repo) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ArticuloResponseDto>> Listar(CancellationToken ct) => repo.Articulos.ListarAsync(ct);

    [HttpGet("buscar")]
    public Task<IReadOnlyList<ResumenArticuloDto>> Buscar([FromQuery] FiltroArticulosDto filtro, CancellationToken ct) =>
        repo.BuscarArticulosAsync(filtro, ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticuloResponseDto>> Obtener(Guid id, CancellationToken ct) =>
        await repo.Articulos.ObtenerAsync(id, ct) is { } item ? Ok(item) : NotFound();

    [HttpGet("{id:guid}/ficha")]
    public async Task<ActionResult<FichaArticuloDto>> Ficha(Guid id, CancellationToken ct) =>
        await repo.ObtenerFichaArticuloAsync(id, ct) is { } ficha ? Ok(ficha) : NotFound();

    [HttpPut("{id:guid}/modalidad")]
    public async Task<IActionResult> CambiarModalidad(Guid id, CambioModalidadRequestDto solicitud, CancellationToken ct)
    {
        var r = await repo.CambiarModalidadAsync(id, solicitud, ct);
        return StatusCode(r.Success ? 200 : r.Estado ?? 400, r);
    }
}
