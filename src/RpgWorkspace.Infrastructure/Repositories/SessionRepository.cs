using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Session>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        => await _context.Sessions.AddAsync(session, cancellationToken);

    public void Remove(Session session)
        => _context.Sessions.Remove(session);
}
