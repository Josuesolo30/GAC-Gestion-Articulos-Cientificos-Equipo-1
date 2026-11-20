using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record PublicacionCongresoResponseDto(Guid Id, string Titulo, string? CorreoContacto,
    IReadOnlyList<Guid> InvestigadorIds, IReadOnlyList<Guid> PalabraClaveIds,
    UbicacionResponseDto? Ubicacion, Guid EdicionCongresoId)
    : ArticuloResponseDto(Id, Titulo, CorreoContacto, 2, InvestigadorIds, PalabraClaveIds, Ubicacion);
