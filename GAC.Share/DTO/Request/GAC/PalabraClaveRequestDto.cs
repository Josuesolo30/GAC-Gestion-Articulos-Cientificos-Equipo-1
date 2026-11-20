using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class PalabraClaveRequestDto
{
    [Required, MaxLength(100)]
    public string Texto { get; init; } = string.Empty;
}
