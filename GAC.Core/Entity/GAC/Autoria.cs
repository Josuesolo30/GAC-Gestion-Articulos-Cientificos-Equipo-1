using System;

namespace GAC.Domain.Entity.GAC;

public sealed class Autoria
{
    public Guid ArticuloId { get; set; }

    public Guid InvestigadorId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    public Investigador Investigador { get; set; } = null!;
}
