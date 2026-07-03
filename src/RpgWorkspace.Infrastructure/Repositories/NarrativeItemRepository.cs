using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class NarrativeItemRepository : INarrativeItemRepository
{
    private readonly AppDbContext _context;

    public NarrativeItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<NarrativeItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.NarrativeItems
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<NarrativeItem>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.NarrativeItems
            .Where(i => i.CharacterId == characterId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NarrativeItem narrativeItem, CancellationToken cancellationToken = default)
        => await _context.NarrativeItems.AddAsync(narrativeItem, cancellationToken);

    public void Remove(NarrativeItem narrativeItem)
        => _context.NarrativeItems.Remove(narrativeItem);
}
