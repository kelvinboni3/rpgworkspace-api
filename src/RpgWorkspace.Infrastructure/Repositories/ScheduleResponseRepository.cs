using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class ScheduleResponseRepository : IScheduleResponseRepository
{
    private readonly AppDbContext _context;

    public ScheduleResponseRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ScheduleResponse?> GetByEventAndUserAsync(
        Guid scheduleEventId, Guid userId, CancellationToken cancellationToken = default)
        => _context.ScheduleResponses
            .FirstOrDefaultAsync(
                r => r.ScheduleEventId == scheduleEventId && r.UserId == userId,
                cancellationToken);

    public async Task AddAsync(ScheduleResponse scheduleResponse, CancellationToken cancellationToken = default)
        => await _context.ScheduleResponses.AddAsync(scheduleResponse, cancellationToken);
}
