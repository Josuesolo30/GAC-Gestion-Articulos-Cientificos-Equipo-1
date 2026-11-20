using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class InvestigadorGrupoRequestDto
{
    [Required]
    public Guid? GrupoInvestigacionId { get; init; }

    [Required]
    public Guid? InvestigadorId { get; init; }
}
