using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class UbicacionRequestDto
{
    [Required]
    public Guid? ArticuloId { get; init; }

    [Required]
    public Guid? GrupoInvestigacionId { get; init; }

    [Range(1, 2)]
    public int Tipo { get; init; }

    [MaxLength(150)]
    public string? Laboratorio { get; init; }

    [MaxLength(100)]
    public string? Estanteria { get; init; }

    public Guid? InvestigadorId { get; init; }

    [MaxLength(150)]
    public string? Despacho { get; init; }
}
