namespace GAC.Domain.Entity.GAC;

public sealed class RevistaTema
{
    public Guid RevistaId { get; set; }
    public Guid TemaId { get; set; }

    // Navegaciones obligatorias: EF Core las completa al cargar la relación.
    public Revista Revista { get; set; } = null!;
    public Tema Tema { get; set; } = null!;
}