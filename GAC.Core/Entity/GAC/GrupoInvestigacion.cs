using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class GrupoInvestigacion
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<InvestigadorGrupo> Miembros { get; set; }
        = new List<InvestigadorGrupo>();

    public ICollection<GrupoTema> Temas { get; set; }
        = new List<GrupoTema>();

    public ICollection<Ubicacion> Ubicaciones { get; set; }
        = new List<Ubicacion>();
}