using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record ColaboracionRevistaResponseDto(Guid RevistaId, Guid InvestigadorId);
