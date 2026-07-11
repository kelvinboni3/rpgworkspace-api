namespace RpgWorkspace.Application.DTOs.CharacterTab;

public sealed record CharacterTabResponse(
    string Id,
    string CharacterId,
    string Name,
    int Order,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
