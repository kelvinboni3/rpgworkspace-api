using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Location>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.CampaignId == campaignId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
        => await _context.Locations.AddAsync(location, cancellationToken);

    public void Remove(Location location)
        => _context.Locations.Remove(location);
}
