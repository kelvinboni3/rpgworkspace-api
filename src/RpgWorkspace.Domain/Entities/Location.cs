using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Location : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public LocationType Type { get; private set; }
    public string? Description { get; private set; }
    public string? Region { get; private set; }
    public ImportanceLevel Importance { get; private set; }
    public bool IsPrivate { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;

    // EF Core constructor
    private Location() { }

    public static Location Create(
        Guid campaignId,
        string name,
        LocationType type,
        string? description,
        string? region,
        ImportanceLevel importance,
        bool isPrivate)
    {
        return new Location
        {
            CampaignId = campaignId,
            Name = name,
            Type = type,
            Description = description,
            Region = region,
            Importance = importance,
            IsPrivate = isPrivate,
        };
    }

    public void Update(
        string name,
        LocationType type,
        string? description,
        string? region,
        ImportanceLevel importance,
        bool isPrivate)
    {
        Name = name;
        Type = type;
        Description = description;
        Region = region;
        Importance = importance;
        IsPrivate = isPrivate;
        SetUpdatedAt();
    }
}
