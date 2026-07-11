using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.CampaignId == campaignId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        Guid campaignId,
        string name,
        Guid? exceptId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return _context.Tags.AnyAsync(
            t => t.CampaignId == campaignId &&
                 t.Name.ToLower() == normalizedName &&
                 (exceptId == null || t.Id != exceptId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        => await _context.Tags.AddAsync(tag, cancellationToken);

    public void Remove(Tag tag)
        => _context.Tags.Remove(tag);

    public async Task<IReadOnlyList<Tag>> GetTagsForEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return entityType switch
        {
            "Session" => await _context.SessionTags.Where(x => x.SessionId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            "Npc" => await _context.NpcTags.Where(x => x.NpcId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            "Location" => await _context.LocationTags.Where(x => x.LocationId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            "Quest" => await _context.QuestTags.Where(x => x.QuestId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            "WikiPage" => await _context.WikiPageTags.Where(x => x.WikiPageId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            "WorldLibraryItem" => await _context.WorldLibraryItemTags.Where(x => x.WorldLibraryItemId == entityId).Select(x => x.Tag).OrderBy(t => t.Name).ToListAsync(cancellationToken),
            _ => throw new InvalidOperationException("Unsupported tag entity type.")
        };
    }

    public async Task ReplaceTagsAsync(
        string entityType,
        Guid entityId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken = default)
    {
        var distinctTagIds = tagIds.Distinct().ToList();

        switch (entityType)
        {
            case "Session":
                var sessionTags = await _context.SessionTags.Where(x => x.SessionId == entityId).ToListAsync(cancellationToken);
                _context.SessionTags.RemoveRange(sessionTags);
                await _context.SessionTags.AddRangeAsync(distinctTagIds.Select(id => SessionTag.Create(entityId, id)), cancellationToken);
                break;
            case "Npc":
                var npcTags = await _context.NpcTags.Where(x => x.NpcId == entityId).ToListAsync(cancellationToken);
                _context.NpcTags.RemoveRange(npcTags);
                await _context.NpcTags.AddRangeAsync(distinctTagIds.Select(id => NpcTag.Create(entityId, id)), cancellationToken);
                break;
            case "Location":
                var locationTags = await _context.LocationTags.Where(x => x.LocationId == entityId).ToListAsync(cancellationToken);
                _context.LocationTags.RemoveRange(locationTags);
                await _context.LocationTags.AddRangeAsync(distinctTagIds.Select(id => LocationTag.Create(entityId, id)), cancellationToken);
                break;
            case "Quest":
                var questTags = await _context.QuestTags.Where(x => x.QuestId == entityId).ToListAsync(cancellationToken);
                _context.QuestTags.RemoveRange(questTags);
                await _context.QuestTags.AddRangeAsync(distinctTagIds.Select(id => QuestTag.Create(entityId, id)), cancellationToken);
                break;
            case "WikiPage":
                var wikiPageTags = await _context.WikiPageTags.Where(x => x.WikiPageId == entityId).ToListAsync(cancellationToken);
                _context.WikiPageTags.RemoveRange(wikiPageTags);
                await _context.WikiPageTags.AddRangeAsync(distinctTagIds.Select(id => WikiPageTag.Create(entityId, id)), cancellationToken);
                break;
            case "WorldLibraryItem":
                var worldLibraryItemTags = await _context.WorldLibraryItemTags.Where(x => x.WorldLibraryItemId == entityId).ToListAsync(cancellationToken);
                _context.WorldLibraryItemTags.RemoveRange(worldLibraryItemTags);
                await _context.WorldLibraryItemTags.AddRangeAsync(distinctTagIds.Select(id => WorldLibraryItemTag.Create(entityId, id)), cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported tag entity type.");
        }
    }
}
