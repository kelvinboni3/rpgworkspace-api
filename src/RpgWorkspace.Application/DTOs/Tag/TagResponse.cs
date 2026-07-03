namespace RpgWorkspace.Application.DTOs.Tag;

public sealed record TagResponse(
    string Id,
    string CampaignId,
    string Name,
    string? Color
);
