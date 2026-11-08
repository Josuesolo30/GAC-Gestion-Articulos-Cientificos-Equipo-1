using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class PublicacionRevista : Articulo
{

    public PublicacionRevista() : base(TipoPublicacion.Revista) { }

    public Guid RevistaId { get; set; }
    public Revista Revista { get; set; } = null!;

    [Required, MaxLength(80)]
    public string NumeroRevista { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PaginaInicial { get; set; }

    [Range(1, int.MaxValue)]
    public int PaginaFinal { get; set; }

    [Range(1, 9999)]
    public int AnioPublicacion { get; set; }

}