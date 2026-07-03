using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Dashboard;

public sealed record CampaignDashboardResponse(
    string CampaignId,
    string CampaignName,
    string? SystemName,
    CampaignDashboardScheduleEventResponse? NextScheduleEvent,
    CampaignDashboardLastSessionResponse? LastSession,
    int ActiveQuestsCount,
    int NpcsCount,
    int LocationsCount,
    int CharactersCount,
    IReadOnlyList<CampaignDashboardSessionResponse> RecentSessions,
    IReadOnlyList<CampaignDashboardNpcResponse> RecentNpcs,
    IReadOnlyList<CampaignDashboardQuestResponse> ActiveQuests
);

public sealed record CampaignDashboardScheduleEventResponse(
    string Id,
    string Title,
    DateTime ProposedDate,
    ScheduleEventStatus Status
);

public sealed record CampaignDashboardLastSessionResponse(
    string Id,
    string Title,
    int Number,
    DateTime Date,
    string? Summary
);

public sealed record CampaignDashboardSessionResponse(
    string Id,
    string Title,
    int Number,
    DateTime Date,
    SessionStatus Status
);

public sealed record CampaignDashboardNpcResponse(
    string Id,
    string Name,
    NpcStatus Status,
    bool IsPrivate
);

public sealed record CampaignDashboardQuestResponse(
    string Id,
    string Title,
    QuestStatus Status,
    bool IsPrivate
);
