using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record InformeTecnicoResponseDto(Guid Id, string Titulo, string? CorreoContacto,
    IReadOnlyList<Guid> InvestigadorIds, IReadOnlyList<Guid> PalabraClaveIds,
    UbicacionResponseDto? Ubicacion, string NumeroInforme, string CentroPublicacion, int MesPublicacion, int AnioPublicacion, Guid TipoInformeId)
    : ArticuloResponseDto(Id, Titulo, CorreoContacto, 1, InvestigadorIds, PalabraClaveIds, Ubicacion);
