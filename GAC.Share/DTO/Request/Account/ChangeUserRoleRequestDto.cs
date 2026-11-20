using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.Account;

public sealed class ChangeUserRoleRequestDto
{
    [Required, EmailAddress]
    public string UserEmail { get; init; } = string.Empty;

    [Required]
    public string RoleId { get; init; } = string.Empty;
}
