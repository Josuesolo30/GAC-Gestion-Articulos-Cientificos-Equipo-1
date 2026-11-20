using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class InvestigadorRequestDto
{
    [Required, MaxLength(200)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(250)]
    public string CentroTrabajo { get; init; } = string.Empty;

    [EmailAddress, MaxLength(254)]
    public string? CorreoElectronico { get; init; }
}
