namespace RpgWorkspace.Application.DTOs.CharacterAttribute;

public sealed record CharacterAttributeResponse(
    string Id,
    string CharacterId,
    string Name,
    string Value,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
