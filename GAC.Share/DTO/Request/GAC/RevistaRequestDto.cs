using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class RevistaRequestDto
{
    [Required, MaxLength(250)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string NombreEditor { get; init; } = string.Empty;

    [Range(1, 9999)]
    public int AnioInicioPublicacion { get; init; }

    [Required, MaxLength(80)]
    public string FrecuenciaPublicacion { get; init; } = string.Empty;
}
