using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.Account;

public sealed class CreateAccountRequestDto
{
    [Required, MaxLength(200)]
    public string NombreCompleto { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s])\S+$")]
    public string Password { get; init; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;
}
