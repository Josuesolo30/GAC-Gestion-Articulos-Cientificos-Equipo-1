using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAC.Domain.Entity.GAC;

public sealed class ColaboracionRevista
{
    public Guid RevistaId { get; set; }

    public Guid InvestigadorId { get; set; }

    public Revista Revista { get; set; } = null!;

    public Investigador Investigador { get; set; } = null!;
}