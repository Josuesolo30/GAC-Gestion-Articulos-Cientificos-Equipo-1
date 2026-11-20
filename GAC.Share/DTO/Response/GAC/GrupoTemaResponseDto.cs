using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record GrupoTemaResponseDto(Guid GrupoInvestigacionId, Guid TemaId);
