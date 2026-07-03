using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Quest : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public QuestStatus Status { get; private set; }
    public string? Reward { get; private set; }
    public bool IsPrivate { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;

    // EF Core constructor
    private Quest() { }

    public static Quest Create(
        Guid campaignId,
        string title,
        string? description,
        QuestStatus status,
        string? reward,
        bool isPrivate)
    {
        return new Quest
        {
            CampaignId = campaignId,
            Title = title,
            Description = description,
            Status = status,
            Reward = reward,
            IsPrivate = isPrivate,
        };
    }

    public void Update(
        string title,
        string? description,
        QuestStatus status,
        string? reward,
        bool isPrivate)
    {
        Title = title;
        Description = description;
        Status = status;
        Reward = reward;
        IsPrivate = isPrivate;
        SetUpdatedAt();
    }
}
