using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.GruposInvestigacion)]
public sealed class GruposInvestigacionController(IGacRepository repo)
    : OperacionesControllerBase<GrupoInvestigacionRequestDto, GrupoInvestigacionResponseDto>(repo.GruposInvestigacion)
{
}
