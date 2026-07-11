namespace RpgWorkspace.Application.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string UserId,
    string Name,
    string Email,
    string? DefaultCharacterId
);
