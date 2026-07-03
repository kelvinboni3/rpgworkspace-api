using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class WorldLibraryItem : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public WorldLibraryCategory Category { get; private set; }
    public string? Description { get; private set; }
    public string? RulesText { get; private set; }
    public string? Notes { get; private set; }
    public WorldLibraryVisibility Visibility { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    // EF Core constructor
    private WorldLibraryItem() { }

    public static WorldLibraryItem Create(
        Guid campaignId,
        string title,
        WorldLibraryCategory category,
        string? description,
        string? rulesText,
        string? notes,
        WorldLibraryVisibility visibility,
        Guid createdByUserId)
    {
        return new WorldLibraryItem
        {
            CampaignId = campaignId,
            Title = title,
            Category = category,
            Description = description,
            RulesText = rulesText,
            Notes = notes,
            Visibility = visibility,
            CreatedByUserId = createdByUserId,
        };
    }

    public void Update(
        string title,
        WorldLibraryCategory category,
        string? description,
        string? rulesText,
        string? notes,
        WorldLibraryVisibility visibility)
    {
        Title = title;
        Category = category;
        Description = description;
        RulesText = rulesText;
        Notes = notes;
        Visibility = visibility;
        SetUpdatedAt();
    }
}
