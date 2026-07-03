using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class PlayerNote : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Tags { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;
    public Session? Session { get; private set; }

    // EF Core constructor
    private PlayerNote() { }

    public static PlayerNote Create(
        Guid characterId,
        Guid? sessionId,
        string title,
        string content,
        string? tags)
    {
        return new PlayerNote
        {
            CharacterId = characterId,
            SessionId = sessionId,
            Title = title,
            Content = content,
            Tags = tags,
        };
    }

    public void Update(Guid? sessionId, string title, string content, string? tags)
    {
        SessionId = sessionId;
        Title = title;
        Content = content;
        Tags = tags;
        SetUpdatedAt();
    }
}
