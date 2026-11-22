using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.InformesTecnicos)]
public sealed class InformesTecnicosController(IConsultaInformesTecnicos consulta) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InformeTecnicoResponseDto>>> Listar(
        CancellationToken cancellationToken)
        => Ok(await consulta.ListarAsync(cancellationToken));
}