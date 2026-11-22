using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class GrupoInvestigacionRequestDto
{
    [Required, MaxLength(200)]
    public string Nombre { get; init; } = string.Empty;
    public IReadOnlyList<Guid>? TemaIds { get; init; }
}
