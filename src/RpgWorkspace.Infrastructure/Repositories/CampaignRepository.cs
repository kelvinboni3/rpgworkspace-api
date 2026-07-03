using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly AppDbContext _context;

    public CampaignRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Campaign>> GetAllByWorkspaceAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
        => await _context.Campaigns.AddAsync(campaign, cancellationToken);

    public void Remove(Campaign campaign)
        => _context.Campaigns.Remove(campaign);
}
