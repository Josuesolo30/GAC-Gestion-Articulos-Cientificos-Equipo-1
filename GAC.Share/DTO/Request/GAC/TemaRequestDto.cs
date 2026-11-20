using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class TemaRequestDto
{
    [Required, MaxLength(150)]
    public string Nombre { get; init; } = string.Empty;
}
