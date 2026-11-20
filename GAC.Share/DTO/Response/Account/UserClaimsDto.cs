using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.Account;

public sealed record UserClaimsDto(string? NombreCompleto, string? UserName, string? Email, string? Role);
