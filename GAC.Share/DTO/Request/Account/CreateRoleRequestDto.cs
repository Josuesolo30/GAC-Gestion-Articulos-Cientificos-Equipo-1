using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Request.Account;

public sealed class CreateRoleRequestDto
{
    [Required, MaxLength(256)]
    public string Name { get; init; } = string.Empty;
}
