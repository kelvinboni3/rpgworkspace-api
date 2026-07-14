namespace RpgWorkspace.Application.DTOs.CharacterTab;

public sealed record CharacterTabResponse(
    string Id,
    string CharacterId,
    string Name,
    int Order,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
