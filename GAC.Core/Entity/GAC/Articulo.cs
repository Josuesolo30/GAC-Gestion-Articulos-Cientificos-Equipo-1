using System.ComponentModel.DataAnnotations;


namespace GAC.Domain.Entity.GAC;

public abstract class Articulo
{
    public Guid Id { get; set; }


    protected Articulo(TipoPublicacion tipo)
    {
        TipoPublicacion = tipo;
    }

    [Required, MaxLength(500)]
    public string Titulo { get; set; } = string.Empty;

    [EmailAddress, MaxLength(254)]
    public string? CorreoContacto { get; set; }

    public TipoPublicacion TipoPublicacion { get; private set; }
    public ICollection<Autoria> Autorias { get; set; } = new List<Autoria>();
    public ICollection<ArticuloPalabraClave> PalabrasClave { get; set; }
        = new List<ArticuloPalabraClave>();
    public Ubicacion? Ubicacion { get; set; }

}