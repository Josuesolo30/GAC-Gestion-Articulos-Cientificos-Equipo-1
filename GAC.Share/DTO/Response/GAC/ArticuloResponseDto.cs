using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public record ArticuloResponseDto(Guid Id, string Titulo, string? CorreoContacto,
    int TipoPublicacion, IReadOnlyList<Guid> InvestigadorIds,
    IReadOnlyList<Guid> PalabraClaveIds, UbicacionResponseDto? Ubicacion);
