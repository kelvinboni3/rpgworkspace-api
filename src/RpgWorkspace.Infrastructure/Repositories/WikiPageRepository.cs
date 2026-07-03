using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class WikiPageRepository : IWikiPageRepository
{
    private readonly AppDbContext _context;

    public WikiPageRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WikiPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.WikiPages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WikiPage>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.WikiPages
            .Where(p => p.CampaignId == campaignId)
            .OrderBy(p => p.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WikiPage wikiPage, CancellationToken cancellationToken = default)
        => await _context.WikiPages.AddAsync(wikiPage, cancellationToken);

    public void Remove(WikiPage wikiPage)
        => _context.WikiPages.Remove(wikiPage);
}
