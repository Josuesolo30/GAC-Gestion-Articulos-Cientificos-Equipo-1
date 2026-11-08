
using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class Ubicacion
{
    public Guid Id { get; set; }

    public Guid ArticuloId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    public Guid GrupoInvestigacionId { get; set; }

    public GrupoInvestigacion GrupoInvestigacion { get; set; }
        = null!;

    [EnumDataType(typeof(TipoUbicacion))]
    public TipoUbicacion Tipo { get; set; }

    [MaxLength(150)]
    public string? Laboratorio { get; set; }

    [MaxLength(100)]
    public string? Estanteria { get; set; }

    public Guid? InvestigadorId { get; set; }

    public Investigador? Investigador { get; set; }

    [MaxLength(150)]
    public string? Despacho { get; set; }
}
