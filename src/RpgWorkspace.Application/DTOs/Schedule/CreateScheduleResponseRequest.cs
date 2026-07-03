using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Schedule;

public sealed record CreateScheduleResponseRequest(
    [Required] ScheduleResponseType Response,
    [MaxLength(500)] string? Comment
);
