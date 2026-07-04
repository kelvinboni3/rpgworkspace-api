namespace RpgWorkspace.Application.DTOs.CharacterTab;

public sealed record CharacterTabResponse(
    string Id,
    string CharacterId,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
