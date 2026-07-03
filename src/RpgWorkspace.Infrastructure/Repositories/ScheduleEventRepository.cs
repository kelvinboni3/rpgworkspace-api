using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class ScheduleEventRepository : IScheduleEventRepository
{
    private readonly AppDbContext _context;

    public ScheduleEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ScheduleEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.ScheduleEvents
            .Include(e => e.Responses)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ScheduleEvent>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleEvents
            .Include(e => e.Responses)
            .Where(e => e.CampaignId == campaignId)
            .OrderBy(e => e.ProposedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ScheduleEvent scheduleEvent, CancellationToken cancellationToken = default)
        => await _context.ScheduleEvents.AddAsync(scheduleEvent, cancellationToken);

    public void Remove(ScheduleEvent scheduleEvent)
        => _context.ScheduleEvents.Remove(scheduleEvent);
}
