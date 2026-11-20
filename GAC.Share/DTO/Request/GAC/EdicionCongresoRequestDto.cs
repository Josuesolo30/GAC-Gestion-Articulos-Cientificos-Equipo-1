using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class EdicionCongresoRequestDto
{
    [Required]
    public Guid? CongresoId { get; init; }

    [Required, MaxLength(80)]
    public string Edicion { get; init; } = string.Empty;

    [Required, MaxLength(150)]
    public string Ciudad { get; init; } = string.Empty;

    [MaxLength(150)]
    public string? Pais { get; init; }

    public DateTime FechaInicio { get; init; }

    public DateTime FechaFin { get; init; }
}
