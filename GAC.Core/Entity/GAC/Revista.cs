using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class Revista
{
    public Guid Id { get; set; }


    [Required, MaxLength(250)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string NombreEditor { get; set; } = string.Empty;

    [Range(1, 9999)]
    public int AnioInicioPublicacion { get; set; }

    [Required, MaxLength(80)]
    public string FrecuenciaPublicacion { get; set; } = string.Empty;

    public ICollection<RevistaTema> Temas { get; set; }
        = new List<RevistaTema>();
    public ICollection<PublicacionRevista> Articulos { get; set; }
        = new List<PublicacionRevista>();
    public ICollection<ColaboracionRevista> Colaboradores
    { get; set; } = new List<ColaboracionRevista>();

}
