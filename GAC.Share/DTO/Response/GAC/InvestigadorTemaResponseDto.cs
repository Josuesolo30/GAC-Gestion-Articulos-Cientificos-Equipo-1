using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record InvestigadorTemaResponseDto(Guid InvestigadorId, Guid TemaId);
