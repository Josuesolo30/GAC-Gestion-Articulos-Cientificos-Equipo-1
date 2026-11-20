using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record InvestigadorGrupoResponseDto(Guid GrupoInvestigacionId, Guid InvestigadorId);
