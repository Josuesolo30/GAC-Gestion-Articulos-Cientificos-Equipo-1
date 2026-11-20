using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class InvestigadorTemaRequestDto
{
    [Required]
    public Guid? InvestigadorId { get; init; }

    [Required]
    public Guid? TemaId { get; init; }
}
