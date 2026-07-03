using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class ScheduleEvent : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime ProposedDate { get; private set; }
    public ScheduleEventStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    public IReadOnlyCollection<ScheduleResponse> Responses => _responses.AsReadOnly();
    private readonly List<ScheduleResponse> _responses = [];

    // EF Core constructor
    private ScheduleEvent() { }

    public static ScheduleEvent Create(
        Guid campaignId,
        string title,
        string? description,
        DateTime proposedDate,
        ScheduleEventStatus status,
        Guid createdByUserId)
    {
        return new ScheduleEvent
        {
            CampaignId = campaignId,
            Title = title,
            Description = description,
            ProposedDate = proposedDate,
            Status = status,
            CreatedByUserId = createdByUserId,
        };
    }

    public void Update(string title, string? description, DateTime proposedDate, ScheduleEventStatus status)
    {
        Title = title;
        Description = description;
        ProposedDate = proposedDate;
        Status = status;
        SetUpdatedAt();
    }
}
