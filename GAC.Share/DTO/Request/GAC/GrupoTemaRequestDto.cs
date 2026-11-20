using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class GrupoTemaRequestDto
{
    [Required]
    public Guid? GrupoInvestigacionId { get; init; }

    [Required]
    public Guid? TemaId { get; init; }
}
