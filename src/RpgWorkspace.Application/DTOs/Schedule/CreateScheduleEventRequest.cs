using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Schedule;

public sealed record CreateScheduleEventRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(1000)] string? Description,
    [Required] DateTime ProposedDate,
    ScheduleEventStatus Status = ScheduleEventStatus.Proposed
);
