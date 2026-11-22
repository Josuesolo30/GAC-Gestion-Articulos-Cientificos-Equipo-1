using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.InformesTecnicos)]
public sealed class InformesTecnicosController(IGacRepository repo)
    : OperacionesControllerBase<InformeTecnicoRequestDto, InformeTecnicoResponseDto>(repo.InformesTecnicos)
{
}
