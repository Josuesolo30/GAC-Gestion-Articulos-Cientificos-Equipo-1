using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record RevistaTemaResponseDto(Guid RevistaId, Guid TemaId);
