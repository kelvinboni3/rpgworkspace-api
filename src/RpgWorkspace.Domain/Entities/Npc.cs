using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Npc : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public NpcStatus Status { get; private set; }
    public bool IsPrivate { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;

    // EF Core constructor
    private Npc() { }

    public static Npc Create(
        Guid campaignId,
        string name,
        string? description,
        NpcStatus status,
        bool isPrivate,
        string? notes)
    {
        return new Npc
        {
            CampaignId = campaignId,
            Name = name,
            Description = description,
            Status = status,
            IsPrivate = isPrivate,
            Notes = notes,
        };
    }

    public void Update(
        string name,
        string? description,
        NpcStatus status,
        bool isPrivate,
        string? notes)
    {
        Name = name;
        Description = description;
        Status = status;
        IsPrivate = isPrivate;
        Notes = notes;
        SetUpdatedAt();
    }
}
