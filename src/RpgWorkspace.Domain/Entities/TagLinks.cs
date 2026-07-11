using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class SessionTag : BaseEntity
{
    public Guid SessionId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private SessionTag() { }
    public static SessionTag Create(Guid sessionId, Guid tagId) => new() { SessionId = sessionId, TagId = tagId };
}

public sealed class NpcTag : BaseEntity
{
    public Guid NpcId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private NpcTag() { }
    public static NpcTag Create(Guid npcId, Guid tagId) => new() { NpcId = npcId, TagId = tagId };
}

public sealed class LocationTag : BaseEntity
{
    public Guid LocationId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private LocationTag() { }
    public static LocationTag Create(Guid locationId, Guid tagId) => new() { LocationId = locationId, TagId = tagId };
}

public sealed class QuestTag : BaseEntity
{
    public Guid QuestId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private QuestTag() { }
    public static QuestTag Create(Guid questId, Guid tagId) => new() { QuestId = questId, TagId = tagId };
}

public sealed class WikiPageTag : BaseEntity
{
    public Guid WikiPageId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private WikiPageTag() { }
    public static WikiPageTag Create(Guid wikiPageId, Guid tagId) => new() { WikiPageId = wikiPageId, TagId = tagId };
}

public sealed class WorldLibraryItemTag : BaseEntity
{
    public Guid WorldLibraryItemId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    private WorldLibraryItemTag() { }
    public static WorldLibraryItemTag Create(Guid worldLibraryItemId, Guid tagId) => new() { WorldLibraryItemId = worldLibraryItemId, TagId = tagId };
}
