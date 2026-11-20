using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class PublicacionCongresoRequestDto : ArticuloBaseRequestDto
{
    [Required]
    public Guid? EdicionCongresoId { get; init; }
}
