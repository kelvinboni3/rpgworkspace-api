using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.NarrativeItem;

public sealed record NarrativeItemResponse(
    string Id,
    string CharacterId,
    string Name,
    string? Description,
    string? Origin,
    string? SessionId,
    NarrativeItemImportance Importance,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
