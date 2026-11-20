using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public abstract class ArticuloBaseRequestDto
{
    [Required, MaxLength(500)]
    public string Titulo { get; init; } = string.Empty;
    [EmailAddress, MaxLength(254)]
    public string? CorreoContacto { get; init; }
    [MinLength(1)]
    public IReadOnlyList<Guid> InvestigadorIds { get; init; } = [];
    [MinLength(1)]
    public IReadOnlyList<Guid> PalabraClaveIds { get; init; } = [];
}
