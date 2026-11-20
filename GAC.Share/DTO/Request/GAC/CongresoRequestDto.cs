using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class CongresoRequestDto
{
    [Required, MaxLength(250)]
    public string Nombre { get; init; } = string.Empty;

    [Range(1, 2)]
    public int Tipo { get; init; }

    [Required, MaxLength(80)]
    public string FrecuenciaCelebracion { get; init; } = string.Empty;

    [Range(1, 9999)]
    public int AnioPrimeraCelebracion { get; init; }
}
