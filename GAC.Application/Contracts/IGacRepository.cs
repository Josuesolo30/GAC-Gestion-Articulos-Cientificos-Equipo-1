namespace GAC.Application.Contracts;

/// <summary>
/// Contrato de las operaciones que implementará Infraestructura para persistir el catálogo.
/// No depende de HTTP. Razor Pages usa IGacService, no este repositorio.
/// </summary>
public interface IGacRepository : IGacCatalogo
{
}
