using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class PublicacionRevistaRequestDto : ArticuloBaseRequestDto
{
    [Required]
    public Guid? RevistaId { get; init; }

    [Required, MaxLength(80)]
    public string NumeroRevista { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PaginaInicial { get; init; }

    [Range(1, int.MaxValue)]
    public int PaginaFinal { get; init; }

    [Range(1, 9999)]
    public int AnioPublicacion { get; init; }
}
