using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.Account;

public sealed class LoginRequestDto
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
