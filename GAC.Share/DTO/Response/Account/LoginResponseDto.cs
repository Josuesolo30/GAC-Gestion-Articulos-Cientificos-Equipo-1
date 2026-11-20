using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.Account;

public sealed record LoginResponseDto(bool Success = false, string? Message = null, string? Token = null, string? RefreshToken = null);
