using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Quest;

public sealed record QuestResponse(
    string Id,
    string CampaignId,
    string Title,
    string? Description,
    QuestStatus Status,
    string? Reward,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
