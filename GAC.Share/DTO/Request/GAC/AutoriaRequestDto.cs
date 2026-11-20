using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class AutoriaRequestDto
{
    [Required]
    public Guid? ArticuloId { get; init; }

    [Required]
    public Guid? InvestigadorId { get; init; }
}
