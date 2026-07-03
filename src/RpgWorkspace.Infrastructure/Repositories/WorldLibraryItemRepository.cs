using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class WorldLibraryItemRepository : IWorldLibraryItemRepository
{
    private readonly AppDbContext _context;

    public WorldLibraryItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorldLibraryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.WorldLibraryItems
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorldLibraryItem>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.WorldLibraryItems
            .Where(i => i.CampaignId == campaignId)
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorldLibraryItem worldLibraryItem, CancellationToken cancellationToken = default)
        => await _context.WorldLibraryItems.AddAsync(worldLibraryItem, cancellationToken);

    public void Remove(WorldLibraryItem worldLibraryItem)
        => _context.WorldLibraryItems.Remove(worldLibraryItem);
}
