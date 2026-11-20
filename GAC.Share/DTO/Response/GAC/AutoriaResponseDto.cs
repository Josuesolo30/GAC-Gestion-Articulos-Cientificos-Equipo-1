using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record AutoriaResponseDto(Guid ArticuloId, Guid InvestigadorId);
