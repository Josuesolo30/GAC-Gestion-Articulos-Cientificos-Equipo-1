using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class Investigador
{
    public Guid Id { get; set; }


    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string CentroTrabajo { get; set; } = string.Empty;

    [EmailAddress, MaxLength(254)]
    public string? CorreoElectronico { get; set; }

    public ICollection<InvestigadorGrupo> Grupos { get; set; }
     = new List<InvestigadorGrupo>();
    public ICollection<ColaboracionRevista> ColaboracionesRevista
    { get; set; } = new List<ColaboracionRevista>();
    public ICollection<Autoria> Autorias { get; set; } = new List<Autoria>();
    public ICollection<InvestigadorTema> Temas { get; set; }
        = new List<InvestigadorTema>();
    public ICollection<Ubicacion> UbicacionesDespacho { get; set; }
        = new List<Ubicacion>();

}
