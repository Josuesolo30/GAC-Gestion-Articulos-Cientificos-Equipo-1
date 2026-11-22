using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class FiltroArticulosDto
{
    [MaxLength(250)] public string? Texto { get; init; }
    [RegularExpression("todos|titulo|autor|palabra")] public string? Campo { get; init; }
    [Range(1, 3)] public int? Tipo { get; init; }
    public bool? TieneCopia { get; init; }
}

public sealed class FiltroInvestigadoresDto
{
    [MaxLength(250)] public string? Texto { get; init; }
    [RegularExpression("todos|nombre|centro|tema")] public string? Campo { get; init; }
}

public sealed class CambioModalidadRequestDto
{
    [Range(1, 3)] public int TipoPublicacion { get; init; }
    [MaxLength(80)] public string? NumeroInforme { get; init; }
    [MaxLength(250)] public string? CentroPublicacion { get; init; }
    [Range(1, 12)] public int? MesPublicacion { get; init; }
    [Range(1, 9999)] public int? AnioPublicacion { get; init; }
    public Guid? TipoInformeId { get; init; }
    public Guid? EdicionCongresoId { get; init; }
    public Guid? RevistaId { get; init; }
    [MaxLength(80)] public string? NumeroRevista { get; init; }
    [Range(1, int.MaxValue)] public int? PaginaInicial { get; init; }
    [Range(1, int.MaxValue)] public int? PaginaFinal { get; init; }
}
