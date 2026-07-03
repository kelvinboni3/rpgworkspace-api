using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class Tag : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;

    // EF Core constructor
    private Tag() { }

    public static Tag Create(Guid campaignId, string name, string? color)
    {
        return new Tag
        {
            CampaignId = campaignId,
            Name = name,
            Color = color,
        };
    }

    public void Update(string name, string? color)
    {
        Name = name;
        Color = color;
        SetUpdatedAt();
    }
}
