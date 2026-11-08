using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class EdicionCongreso
{
    public Guid Id { get; set; }


    public Guid CongresoId { get; set; }
    public Congreso Congreso { get; set; } = null!;

    [Required, MaxLength(80)]
    public string Edicion { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Pais { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public ICollection<PublicacionCongreso> Articulos { get; set; }
        = new List<PublicacionCongreso>();

}
