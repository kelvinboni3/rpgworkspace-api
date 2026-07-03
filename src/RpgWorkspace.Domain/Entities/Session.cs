using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Session : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public DateTime Date { get; private set; }
    public string? Summary { get; private set; }
    public string? Notes { get; private set; }
    public SessionStatus Status { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;

    // EF Core constructor
    private Session() { }

    public static Session Create(
        Guid campaignId,
        string title,
        int number,
        DateTime date,
        string? summary,
        string? notes,
        SessionStatus status)
    {
        return new Session
        {
            CampaignId = campaignId,
            Title = title,
            Number = number,
            Date = date,
            Summary = summary,
            Notes = notes,
            Status = status,
        };
    }

    public void Update(
        string title,
        int number,
        DateTime date,
        string? summary,
        string? notes,
        SessionStatus status)
    {
        Title = title;
        Number = number;
        Date = date;
        Summary = summary;
        Notes = notes;
        Status = status;
        SetUpdatedAt();
    }
}
