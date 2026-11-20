using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.Account;

public sealed record UserWithRoleResponseDto(string Id, string NombreCompleto, string? Email, string? RoleName, string? RoleId);
