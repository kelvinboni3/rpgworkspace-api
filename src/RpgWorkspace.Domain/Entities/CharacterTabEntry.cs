using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterTabEntry : BaseEntity
{
    public Guid CharacterTabId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    // Navigation
    public CharacterTab CharacterTab { get; private set; } = null!;

    // EF Core constructor
    private CharacterTabEntry() { }

    public static CharacterTabEntry Create(Guid characterTabId, string title, string content)
    {
        return new CharacterTabEntry
        {
            CharacterTabId = characterTabId,
            Title = title,
            Content = content,
        };
    }

    public void Update(string title, string content)
    {
        Title = title;
        Content = content;
        SetUpdatedAt();
    }
}
