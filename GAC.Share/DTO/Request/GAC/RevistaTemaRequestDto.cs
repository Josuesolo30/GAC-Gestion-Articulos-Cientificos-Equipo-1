using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class RevistaTemaRequestDto
{
    [Required]
    public Guid? RevistaId { get; init; }

    [Required]
    public Guid? TemaId { get; init; }
}
