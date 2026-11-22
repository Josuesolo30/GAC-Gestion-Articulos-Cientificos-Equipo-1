using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.GruposTemas)]
public sealed class GruposTemasController(IGacRepository repo)
    : VinculosControllerBase<GrupoTemaRequestDto, GrupoTemaResponseDto>(repo.GruposTemas)
{
}
