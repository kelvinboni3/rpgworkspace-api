namespace RpgWorkspace.Application.DTOs.Campaign;

public sealed record CampaignResponse(
    string Id,
    string WorkspaceId,
    string Name,
    string? Description,
    string? SystemName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
