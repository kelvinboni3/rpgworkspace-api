using RpgWorkspace.Application.DTOs.Schedule;

namespace RpgWorkspace.Application.Interfaces;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleEventResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ScheduleEventResponse> GetByIdAsync(Guid scheduleEventId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ScheduleEventResponse> CreateAsync(Guid campaignId, CreateScheduleEventRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ScheduleEventResponse> UpdateAsync(Guid scheduleEventId, UpdateScheduleEventRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid scheduleEventId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ScheduleResponseResponse> RespondAsync(Guid scheduleEventId, CreateScheduleResponseRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
}
