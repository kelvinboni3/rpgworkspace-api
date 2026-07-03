namespace RpgWorkspace.Application.DTOs.PlayerNote;

using RpgWorkspace.Application.DTOs.Tag;

public sealed record PlayerNoteResponse(
    string Id,
    string CharacterId,
    string? SessionId,
    string Title,
    string Content,
    string? TagsText,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
