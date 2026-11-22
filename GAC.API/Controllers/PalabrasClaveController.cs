using GAC.Application.Constants;
using GAC.Application.Contracts;
using GAC.Share.DTO.Request.GAC;
using GAC.Share.DTO.Response.GAC;
using Microsoft.AspNetCore.Mvc;

namespace GAC.API.Controllers;

[ApiController]
[Route(GacApiRoutes.PalabrasClave)]
public sealed class PalabrasClaveController(IGacRepository repo)
    : OperacionesControllerBase<PalabraClaveRequestDto, PalabraClaveResponseDto>(repo.PalabrasClave)
{
}
