namespace GAC.Domain.Entity.GAC;

public sealed class InvestigadorTema
{
    public Guid InvestigadorId { get; set; }
    public Guid TemaId { get; set; }

    // Navegaciones obligatorias: EF Core las completa al cargar la relación.
    public Investigador Investigador { get; set; } = null!;
    public Tema Tema { get; set; } = null!;
}