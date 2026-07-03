using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Schedule;

public sealed record ScheduleResponseResponse(
    string Id,
    string ScheduleEventId,
    string UserId,
    ScheduleResponseType Response,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
