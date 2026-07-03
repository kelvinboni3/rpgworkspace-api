using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class NpcRepository : INpcRepository
{
    private readonly AppDbContext _context;

    public NpcRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Npc?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Npcs
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Npc>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Npcs
            .Where(n => n.CampaignId == campaignId)
            .OrderBy(n => n.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Npc npc, CancellationToken cancellationToken = default)
        => await _context.Npcs.AddAsync(npc, cancellationToken);

    public void Remove(Npc npc)
        => _context.Npcs.Remove(npc);
}
