using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IScheduleResponseRepository
{
    Task<ScheduleResponse?> GetByEventAndUserAsync(Guid scheduleEventId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(ScheduleResponse scheduleResponse, CancellationToken cancellationToken = default);
}
