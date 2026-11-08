using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class InformeTecnico : Articulo
{
    public InformeTecnico()
        : base(TipoPublicacion.InformeTecnico)
    {
    }

    [Required, MaxLength(80)]
    public string NumeroInforme { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string CentroPublicacion { get; set; } = string.Empty;

    [Range(1, 12)]
    public int MesPublicacion { get; set; }

    [Range(1, 9999)]
    public int AnioPublicacion { get; set; }

    public Guid TipoInformeId { get; set; }

    public TipoInforme TipoInforme { get; set; } = null!;
}