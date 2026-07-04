using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterTab : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Navigation
    public Character Character { get; private set; } = null!;

    // EF Core constructor
    private CharacterTab() { }

    public static CharacterTab Create(Guid characterId, string name)
    {
        return new CharacterTab
        {
            CharacterId = characterId,
            Name = name,
        };
    }

    public void Update(string name)
    {
        Name = name;
        SetUpdatedAt();
    }
}
