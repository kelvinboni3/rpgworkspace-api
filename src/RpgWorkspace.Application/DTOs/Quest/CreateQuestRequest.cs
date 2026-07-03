using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Quest;

public sealed record CreateQuestRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(1000)] string? Description,
    QuestStatus Status = QuestStatus.NotStarted,
    [MaxLength(500)] string? Reward = null,
    bool IsPrivate = false,
    Guid[]? TagIds = null
);
