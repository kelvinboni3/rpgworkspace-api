namespace RpgWorkspace.Application.DTOs.CharacterTabEntry;

public sealed record CharacterTabEntryResponse(
    string Id,
    string CharacterTabId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
