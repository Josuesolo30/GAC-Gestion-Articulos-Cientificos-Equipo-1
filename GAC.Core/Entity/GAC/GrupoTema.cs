using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAC.Domain.Entity.GAC;

public sealed class GrupoTema
{
    public Guid GrupoInvestigacionId { get; set; }

    public Guid TemaId { get; set; }

    public GrupoInvestigacion GrupoInvestigacion { get; set; }
        = null!;

    public Tema Tema { get; set; } = null!;
}