using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class NarrativeItem : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Origin { get; private set; }
    public Guid? SessionId { get; private set; }
    public NarrativeItemImportance Importance { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;
    public Session? Session { get; private set; }

    // EF Core constructor
    private NarrativeItem() { }

    public static NarrativeItem Create(
        Guid characterId,
        string name,
        string? description,
        string? origin,
        Guid? sessionId,
        NarrativeItemImportance importance,
        string? notes)
    {
        return new NarrativeItem
        {
            CharacterId = characterId,
            Name = name,
            Description = description,
            Origin = origin,
            SessionId = sessionId,
            Importance = importance,
            Notes = notes,
        };
    }

    public void Update(
        string name,
        string? description,
        string? origin,
        Guid? sessionId,
        NarrativeItemImportance importance,
        string? notes)
    {
        Name = name;
        Description = description;
        Origin = origin;
        SessionId = sessionId;
        Importance = importance;
        Notes = notes;
        SetUpdatedAt();
    }
}
