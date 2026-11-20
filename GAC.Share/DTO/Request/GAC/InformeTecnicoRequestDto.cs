using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class InformeTecnicoRequestDto : ArticuloBaseRequestDto
{
    [Required, MaxLength(80)]
    public string NumeroInforme { get; init; } = string.Empty;

    [Required, MaxLength(250)]
    public string CentroPublicacion { get; init; } = string.Empty;

    [Range(1, 12)]
    public int MesPublicacion { get; init; }

    [Range(1, 9999)]
    public int AnioPublicacion { get; init; }

    [Required]
    public Guid? TipoInformeId { get; init; }
}
