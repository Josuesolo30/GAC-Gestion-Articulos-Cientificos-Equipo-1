using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.GAC;

public sealed class Congreso
{
    public Guid Id { get; set; }


    [Required, MaxLength(250)]
    public string Nombre { get; set; } = string.Empty;

    [EnumDataType(typeof(TipoCongreso))]
    public TipoCongreso Tipo { get; set; }

    [Required, MaxLength(80)]
    public string FrecuenciaCelebracion { get; set; } = string.Empty;

    [Range(1, 9999)]
    public int AnioPrimeraCelebracion { get; set; }

    public ICollection<EdicionCongreso> Ediciones { get; set; }
        = new List<EdicionCongreso>();

}