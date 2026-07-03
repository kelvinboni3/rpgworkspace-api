using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Theory : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Evidence { get; private set; }
    public int Confidence { get; private set; }
    public TheoryStatus Status { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;

    // EF Core constructor
    private Theory() { }

    public static Theory Create(
        Guid characterId,
        string title,
        string? description,
        string? evidence,
        int confidence,
        TheoryStatus status)
    {
        return new Theory
        {
            CharacterId = characterId,
            Title = title,
            Description = description,
            Evidence = evidence,
            Confidence = confidence,
            Status = status,
        };
    }

    public void Update(
        string title,
        string? description,
        string? evidence,
        int confidence,
        TheoryStatus status)
    {
        Title = title;
        Description = description;
        Evidence = evidence;
        Confidence = confidence;
        Status = status;
        SetUpdatedAt();
    }
}
