using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterAttribute : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    // Navigation
    public Character Character { get; private set; } = null!;

    // EF Core constructor
    private CharacterAttribute() { }

    public static CharacterAttribute Create(Guid characterId, string name, string value)
    {
        return new CharacterAttribute
        {
            CharacterId = characterId,
            Name = name,
            Value = value,
        };
    }

    public void Update(string name, string value)
    {
        Name = name;
        Value = value;
        SetUpdatedAt();
    }
}
