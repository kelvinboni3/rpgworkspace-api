using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class Campaign : BaseEntity
{
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? SystemName { get; private set; }

    // Navigation
    public Workspace Workspace { get; private set; } = null!;

    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    private readonly List<Session> _sessions = [];

    public IReadOnlyCollection<Npc> Npcs => _npcs.AsReadOnly();
    private readonly List<Npc> _npcs = [];

    public IReadOnlyCollection<Location> Locations => _locations.AsReadOnly();
    private readonly List<Location> _locations = [];

    public IReadOnlyCollection<Quest> Quests => _quests.AsReadOnly();
    private readonly List<Quest> _quests = [];

    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();
    private readonly List<Character> _characters = [];

    public IReadOnlyCollection<ScheduleEvent> ScheduleEvents => _scheduleEvents.AsReadOnly();
    private readonly List<ScheduleEvent> _scheduleEvents = [];

    public IReadOnlyCollection<WikiPage> WikiPages => _wikiPages.AsReadOnly();
    private readonly List<WikiPage> _wikiPages = [];

    public IReadOnlyCollection<WorldLibraryItem> WorldLibraryItems => _worldLibraryItems.AsReadOnly();
    private readonly List<WorldLibraryItem> _worldLibraryItems = [];

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    private readonly List<Tag> _tags = [];

    // EF Core constructor
    private Campaign() { }

    public static Campaign Create(Guid workspaceId, string name, string? description, string? systemName)
    {
        return new Campaign
        {
            WorkspaceId = workspaceId,
            Name = name,
            Description = description,
            SystemName = systemName,
        };
    }

    public void Update(string name, string? description, string? systemName)
    {
        Name = name;
        Description = description;
        SystemName = systemName;
        SetUpdatedAt();
    }
}
