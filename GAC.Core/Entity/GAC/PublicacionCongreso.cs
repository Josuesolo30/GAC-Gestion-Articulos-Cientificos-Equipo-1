using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class PublicacionCongreso : Articulo
{

    public PublicacionCongreso() : base(TipoPublicacion.Congreso) { }

    public Guid EdicionCongresoId { get; set; }
    public EdicionCongreso EdicionCongreso { get; set; } = null!;

}