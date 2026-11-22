namespace GAC.Share.DTO.Response;

public sealed record GeneralResponseDto(
    bool Success = false, string? Message = null, Guid? Id = null,
    IReadOnlyDictionary<string, string[]>? Errors = null, int? Estado = null);
