using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record InvestigadorResponseDto(Guid Id, string Nombre, string CentroTrabajo, string? CorreoElectronico, IReadOnlyList<Guid>? TemaIds = null, IReadOnlyList<Guid>? GrupoIds = null);
