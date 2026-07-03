using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class QuestRepository : IQuestRepository
{
    private readonly AppDbContext _context;

    public QuestRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Quest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Quests
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Quest>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Quests
            .Where(q => q.CampaignId == campaignId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Quest quest, CancellationToken cancellationToken = default)
        => await _context.Quests.AddAsync(quest, cancellationToken);

    public void Remove(Quest quest)
        => _context.Quests.Remove(quest);
}
