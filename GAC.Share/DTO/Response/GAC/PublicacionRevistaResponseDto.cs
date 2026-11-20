using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record PublicacionRevistaResponseDto(Guid Id, string Titulo, string? CorreoContacto,
    IReadOnlyList<Guid> InvestigadorIds, IReadOnlyList<Guid> PalabraClaveIds,
    UbicacionResponseDto? Ubicacion, Guid RevistaId, string NumeroRevista, int PaginaInicial, int PaginaFinal, int AnioPublicacion)
    : ArticuloResponseDto(Id, Titulo, CorreoContacto, 3, InvestigadorIds, PalabraClaveIds, Ubicacion);
