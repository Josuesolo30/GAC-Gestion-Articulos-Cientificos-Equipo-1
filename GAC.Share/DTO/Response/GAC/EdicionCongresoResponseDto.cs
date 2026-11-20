using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record EdicionCongresoResponseDto(Guid Id, Guid CongresoId, string Edicion, string Ciudad, string? Pais, DateTime FechaInicio, DateTime FechaFin);
