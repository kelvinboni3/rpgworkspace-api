using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Dashboard;

public sealed record CharacterDashboardResponse(
    string CharacterId,
    string CharacterName,
    string CampaignId,
    string CampaignName,
    IReadOnlyList<CharacterDashboardTabSummaryResponse> TabSummaries,
    IReadOnlyList<CharacterDashboardRecentBlockResponse> RecentBlocks
);

public sealed record CharacterDashboardTabSummaryResponse(
    string TabId,
    string TabName,
    int BlockCount
);

public sealed record CharacterDashboardRecentBlockResponse(
    string Id,
    string TabId,
    string TabName,
    CharacterTabBlockType Type,
    string? Title,
    DateTime UpdatedAt
);
