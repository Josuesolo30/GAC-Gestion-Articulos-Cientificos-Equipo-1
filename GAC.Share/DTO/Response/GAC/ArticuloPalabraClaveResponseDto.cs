using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.GAC;

public sealed record ArticuloPalabraClaveResponseDto(Guid ArticuloId, Guid PalabraClaveId);
