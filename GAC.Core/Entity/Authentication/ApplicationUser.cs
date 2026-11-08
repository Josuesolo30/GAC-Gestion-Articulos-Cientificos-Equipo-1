using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GAC.Domain.Entity.Authentication;

public sealed class ApplicationUser : IdentityUser
{
    [Required, MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}