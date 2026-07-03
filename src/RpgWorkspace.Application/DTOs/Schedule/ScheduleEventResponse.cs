using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Schedule;

public sealed record ScheduleEventResponse(
    string Id,
    string CampaignId,
    string Title,
    string? Description,
    DateTime ProposedDate,
    ScheduleEventStatus Status,
    string CreatedByUserId,
    IReadOnlyList<ScheduleResponseResponse> Responses,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
