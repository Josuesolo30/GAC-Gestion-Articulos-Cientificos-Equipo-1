using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.Account;

public sealed class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
