using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class PalabraClave
{
    public Guid Id { get; set; }


    [Required, MaxLength(100)]
    public string Texto { get; set; } = string.Empty;

    public ICollection<ArticuloPalabraClave> Articulos { get; set; }
        = new List<ArticuloPalabraClave>();

}
