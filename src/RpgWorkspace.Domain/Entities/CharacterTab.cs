using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterTab : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsPublic { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;

    public IReadOnlyCollection<CharacterTabBlock> Blocks => _blocks.AsReadOnly();
    private readonly List<CharacterTabBlock> _blocks = [];

    // EF Core constructor
    private CharacterTab() { }

    public static CharacterTab Create(Guid characterId, string name, int order)
    {
        return new CharacterTab
        {
            CharacterId = characterId,
            Name = name,
            Order = order,
        };
    }

    public void Update(string name)
    {
        Name = name;
        SetUpdatedAt();
    }

    public void SetPublic(bool isPublic)
    {
        IsPublic = isPublic;
        SetUpdatedAt();
    }
}
