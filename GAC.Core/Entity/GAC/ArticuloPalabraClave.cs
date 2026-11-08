namespace GAC.Domain.Entity.GAC;

public sealed class ArticuloPalabraClave
{
    public Guid ArticuloId { get; set; }
    public Guid PalabraClaveId { get; set; }

    // Navegaciones obligatorias: EF Core las completa al cargar la relación.
    public Articulo Articulo { get; set; } = null!;
    public PalabraClave PalabraClave { get; set; } = null!;
}