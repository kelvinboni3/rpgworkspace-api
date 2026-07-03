using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class ScheduleResponse : BaseEntity
{
    public Guid ScheduleEventId { get; private set; }
    public Guid UserId { get; private set; }
    public ScheduleResponseType Response { get; private set; }
    public string? Comment { get; private set; }

    // Navigation
    public ScheduleEvent ScheduleEvent { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // EF Core constructor
    private ScheduleResponse() { }

    public static ScheduleResponse Create(
        Guid scheduleEventId,
        Guid userId,
        ScheduleResponseType response,
        string? comment)
    {
        return new ScheduleResponse
        {
            ScheduleEventId = scheduleEventId,
            UserId = userId,
            Response = response,
            Comment = comment,
        };
    }

    public void Update(ScheduleResponseType response, string? comment)
    {
        Response = response;
        Comment = comment;
        SetUpdatedAt();
    }
}
