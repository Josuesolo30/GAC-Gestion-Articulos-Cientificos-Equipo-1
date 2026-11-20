using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.GAC;

public sealed class ArticuloPalabraClaveRequestDto
{
    [Required]
    public Guid? ArticuloId { get; init; }

    [Required]
    public Guid? PalabraClaveId { get; init; }
}
