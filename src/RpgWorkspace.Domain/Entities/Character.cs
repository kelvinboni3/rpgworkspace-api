using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Character : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Race { get; private set; }
    public string? Class { get; private set; }
    public int Level { get; private set; }
    public CharacterStatus Status { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;
    public User User { get; private set; } = null!;

    public IReadOnlyCollection<PlayerNote> PlayerNotes => _playerNotes.AsReadOnly();
    private readonly List<PlayerNote> _playerNotes = [];

    public IReadOnlyCollection<ImportantPerson> ImportantPeople => _importantPeople.AsReadOnly();
    private readonly List<ImportantPerson> _importantPeople = [];

    public IReadOnlyCollection<Theory> Theories => _theories.AsReadOnly();
    private readonly List<Theory> _theories = [];

    public IReadOnlyCollection<Operation> Operations => _operations.AsReadOnly();
    private readonly List<Operation> _operations = [];

    public IReadOnlyCollection<NarrativeItem> NarrativeItems => _narrativeItems.AsReadOnly();
    private readonly List<NarrativeItem> _narrativeItems = [];

    // EF Core constructor
    private Character() { }

    public static Character Create(
        Guid campaignId,
        Guid userId,
        string name,
        string? description,
        string? race,
        string? characterClass,
        int level,
        CharacterStatus status)
    {
        return new Character
        {
            CampaignId = campaignId,
            UserId = userId,
            Name = name,
            Description = description,
            Race = race,
            Class = characterClass,
            Level = level,
            Status = status,
        };
    }

    public void Update(
        string name,
        string? description,
        string? race,
        string? characterClass,
        int level,
        CharacterStatus status)
    {
        Name = name;
        Description = description;
        Race = race;
        Class = characterClass;
        Level = level;
        Status = status;
        SetUpdatedAt();
    }
}
