using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.Authentication;

public sealed class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
}