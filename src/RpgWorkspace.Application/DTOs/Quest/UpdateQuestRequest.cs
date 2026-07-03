using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Quest;

public sealed record UpdateQuestRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(1000)] string? Description,
    [Required] QuestStatus Status,
    [MaxLength(500)] string? Reward,
    bool IsPrivate,
    Guid[]? TagIds = null
);
