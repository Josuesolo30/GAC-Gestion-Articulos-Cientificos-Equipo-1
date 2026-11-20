using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class ColaboracionRevistaRequestDto
{
    [Required]
    public Guid? RevistaId { get; init; }

    [Required]
    public Guid? InvestigadorId { get; init; }
}
