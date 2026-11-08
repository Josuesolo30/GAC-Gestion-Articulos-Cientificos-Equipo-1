using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class TipoInforme
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<InformeTecnico> Informes { get; set; }
        = new List<InformeTecnico>();
}