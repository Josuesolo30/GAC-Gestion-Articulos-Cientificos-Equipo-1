using System.ComponentModel.DataAnnotations;

namespace GAC.Share.DTO.Response.Account;

public sealed record TokenPairDto(string Token, string RefreshToken);
