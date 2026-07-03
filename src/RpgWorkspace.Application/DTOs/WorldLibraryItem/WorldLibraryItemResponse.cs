using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.WorldLibraryItem;

public sealed record WorldLibraryItemResponse(
    string Id,
    string CampaignId,
    string Title,
    WorldLibraryCategory Category,
    string? Description,
    string? RulesText,
    string? Notes,
    WorldLibraryVisibility Visibility,
    string CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
