using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class WikiPage : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public WikiVisibility Visibility { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    // EF Core constructor
    private WikiPage() { }

    public static WikiPage Create(
        Guid campaignId,
        string title,
        string content,
        WikiVisibility visibility,
        Guid createdByUserId)
    {
        return new WikiPage
        {
            CampaignId = campaignId,
            Title = title,
            Content = content,
            Visibility = visibility,
            CreatedByUserId = createdByUserId,
        };
    }

    public void Update(string title, string content, WikiVisibility visibility)
    {
        Title = title;
        Content = content;
        Visibility = visibility;
        SetUpdatedAt();
    }
}
