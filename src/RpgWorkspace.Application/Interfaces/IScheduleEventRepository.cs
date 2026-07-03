using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IScheduleEventRepository
{
    Task<ScheduleEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleEvent>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(ScheduleEvent scheduleEvent, CancellationToken cancellationToken = default);
    void Remove(ScheduleEvent scheduleEvent);
}
