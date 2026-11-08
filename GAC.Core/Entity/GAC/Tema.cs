using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class Tema
{
    public Guid Id { get; set; }


    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<InvestigadorTema> Investigadores { get; set; }
        = new List<InvestigadorTema>();
    public ICollection<RevistaTema> Revistas { get; set; }
        = new List<RevistaTema>();
    public ICollection<GrupoTema> Grupos { get; set; }
    = new List<GrupoTema>();
}