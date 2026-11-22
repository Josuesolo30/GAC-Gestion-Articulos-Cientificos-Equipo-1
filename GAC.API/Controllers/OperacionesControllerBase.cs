using GAC.Application.Contracts;
using GAC.Share.DTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

public abstract class OperacionesControllerBase<TSolicitud, TRespuesta>(ICatalogoCrud<TSolicitud, TRespuesta, Guid> repo) : ControllerBase
    where TSolicitud : class where TRespuesta : class
{
    [HttpGet]
    public Task<IReadOnlyList<TRespuesta>> Listar(CancellationToken ct) => repo.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TRespuesta>> Obtener(Guid id, CancellationToken ct) =>
        await repo.ObtenerAsync(id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(TSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await repo.CrearAsync(solicitud, ct);
        return resultado.Success ? CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, resultado) : Estado(resultado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, TSolicitud solicitud, CancellationToken ct) =>
        Estado(await repo.ActualizarAsync(id, solicitud, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct) =>
        Estado(await repo.EliminarAsync(id, ct));

    protected IActionResult Estado(GeneralResponseDto resultado) =>
        StatusCode(resultado.Success ? 200 : resultado.Estado ?? 400, resultado);
}

public abstract class VinculosControllerBase<TSolicitud, TRespuesta>(ICatalogoCrud<TSolicitud, TRespuesta, (Guid, Guid)> repo) : ControllerBase
    where TSolicitud : class where TRespuesta : class
{
    [HttpGet]
    public Task<IReadOnlyList<TRespuesta>> Listar(CancellationToken ct) => repo.ListarAsync(ct);

    [HttpGet("{primero:guid}/{segundo:guid}")]
    public async Task<ActionResult<TRespuesta>> Obtener(Guid primero, Guid segundo, CancellationToken ct) =>
        await repo.ObtenerAsync((primero, segundo), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(TSolicitud solicitud, CancellationToken ct) => Estado(await repo.CrearAsync(solicitud, ct));

    [HttpPut("{primero:guid}/{segundo:guid}")]
    public async Task<IActionResult> Actualizar(Guid primero, Guid segundo, TSolicitud solicitud, CancellationToken ct) =>
        Estado(await repo.ActualizarAsync((primero, segundo), solicitud, ct));

    [HttpDelete("{primero:guid}/{segundo:guid}")]
    public async Task<IActionResult> Eliminar(Guid primero, Guid segundo, CancellationToken ct) =>
        Estado(await repo.EliminarAsync((primero, segundo), ct));

    private IActionResult Estado(GeneralResponseDto resultado) =>
        StatusCode(resultado.Success ? 200 : resultado.Estado ?? 400, resultado);
}
