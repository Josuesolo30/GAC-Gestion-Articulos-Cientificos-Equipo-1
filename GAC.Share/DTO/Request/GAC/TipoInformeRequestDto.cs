using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class TipoInformeRequestDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;
}
