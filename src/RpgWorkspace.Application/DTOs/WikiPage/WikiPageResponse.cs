using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.WikiPage;

public sealed record WikiPageResponse(
    string Id,
    string CampaignId,
    string Title,
    string Content,
    WikiVisibility Visibility,
    string CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
