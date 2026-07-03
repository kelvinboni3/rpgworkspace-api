using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Location;

public sealed record LocationResponse(
    string Id,
    string CampaignId,
    string Name,
    LocationType Type,
    string? Description,
    string? Region,
    ImportanceLevel Importance,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
