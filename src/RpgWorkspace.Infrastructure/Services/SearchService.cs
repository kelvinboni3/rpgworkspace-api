using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.DTOs.Search;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Services;

public sealed class SearchService : ISearchService
{
    private const int MaxResults = 50;

    private readonly AppDbContext _context;

    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SearchResultResponse>> SearchAsync(
        Guid campaignId,
        string? term,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var workspace = await _context.Workspaces
            .AsNoTracking()
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == campaign.WorkspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Campaign not found.");

        var normalizedTerm = term?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
            return [];

        var canViewPrivate = workspace.IsOwnerOrMaster(requestingUserId);
        var pattern = $"%{normalizedTerm}%";
        var results = new List<SearchResultResponse>();

        results.AddRange(await SearchSessionsAsync(campaignId, pattern, cancellationToken));
        results.AddRange(await SearchNpcsAsync(campaignId, pattern, canViewPrivate, cancellationToken));
        results.AddRange(await SearchLocationsAsync(campaignId, pattern, canViewPrivate, cancellationToken));
        results.AddRange(await SearchQuestsAsync(campaignId, pattern, canViewPrivate, cancellationToken));
        results.AddRange(await SearchCharactersAsync(campaignId, pattern, cancellationToken));
        results.AddRange(await SearchWikiPagesAsync(campaignId, pattern, canViewPrivate, cancellationToken));
        results.AddRange(await SearchWorldLibraryItemsAsync(campaignId, pattern, canViewPrivate, cancellationToken));

        return results
            .OrderBy(r => r.Type)
            .ThenBy(r => r.Title)
            .Take(MaxResults)
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchSessionsAsync(
        Guid campaignId, string pattern, CancellationToken cancellationToken)
    {
        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.CampaignId == campaignId &&
                        (EF.Functions.ILike(s.Title, pattern) ||
                         (s.Summary != null && EF.Functions.ILike(s.Summary, pattern)) ||
                         (s.Notes != null && EF.Functions.ILike(s.Notes, pattern))))
            .Take(MaxResults)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => Result(s.Id, "Session", s.Title, s.Summary, $"/api/sessions/{s.Id}", false))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchNpcsAsync(
        Guid campaignId, string pattern, bool canViewPrivate, CancellationToken cancellationToken)
    {
        var query = _context.Npcs
            .AsNoTracking()
            .Where(n => n.CampaignId == campaignId &&
                        (EF.Functions.ILike(n.Name, pattern) ||
                         (n.Description != null && EF.Functions.ILike(n.Description, pattern)) ||
                         (n.Notes != null && EF.Functions.ILike(n.Notes, pattern))));

        if (!canViewPrivate)
            query = query.Where(n => !n.IsPrivate);

        var npcs = await query.Take(MaxResults).ToListAsync(cancellationToken);

        return npcs
            .Select(n => Result(n.Id, "Npc", n.Name, n.Description, $"/api/npcs/{n.Id}", n.IsPrivate))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchLocationsAsync(
        Guid campaignId, string pattern, bool canViewPrivate, CancellationToken cancellationToken)
    {
        var query = _context.Locations
            .AsNoTracking()
            .Where(l => l.CampaignId == campaignId &&
                        (EF.Functions.ILike(l.Name, pattern) ||
                         (l.Description != null && EF.Functions.ILike(l.Description, pattern)) ||
                         (l.Region != null && EF.Functions.ILike(l.Region, pattern))));

        if (!canViewPrivate)
            query = query.Where(l => !l.IsPrivate);

        var locations = await query.Take(MaxResults).ToListAsync(cancellationToken);

        return locations
            .Select(l => Result(l.Id, "Location", l.Name, l.Description, $"/api/locations/{l.Id}", l.IsPrivate))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchQuestsAsync(
        Guid campaignId, string pattern, bool canViewPrivate, CancellationToken cancellationToken)
    {
        var query = _context.Quests
            .AsNoTracking()
            .Where(q => q.CampaignId == campaignId &&
                        (EF.Functions.ILike(q.Title, pattern) ||
                         (q.Description != null && EF.Functions.ILike(q.Description, pattern)) ||
                         (q.Reward != null && EF.Functions.ILike(q.Reward, pattern))));

        if (!canViewPrivate)
            query = query.Where(q => !q.IsPrivate);

        var quests = await query.Take(MaxResults).ToListAsync(cancellationToken);

        return quests
            .Select(q => Result(q.Id, "Quest", q.Title, q.Description, $"/api/quests/{q.Id}", q.IsPrivate))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchCharactersAsync(
        Guid campaignId, string pattern, CancellationToken cancellationToken)
    {
        var characters = await _context.Characters
            .AsNoTracking()
            .Where(c => c.CampaignId == campaignId &&
                        (EF.Functions.ILike(c.Name, pattern) ||
                         (c.Description != null && EF.Functions.ILike(c.Description, pattern)) ||
                         (c.Race != null && EF.Functions.ILike(c.Race, pattern)) ||
                         (c.Class != null && EF.Functions.ILike(c.Class, pattern))))
            .Take(MaxResults)
            .ToListAsync(cancellationToken);

        return characters
            .Select(c => Result(c.Id, "Character", c.Name, c.Description, $"/api/characters/{c.Id}", false))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchWikiPagesAsync(
        Guid campaignId, string pattern, bool canViewPrivate, CancellationToken cancellationToken)
    {
        var query = _context.WikiPages
            .AsNoTracking()
            .Where(p => p.CampaignId == campaignId &&
                        (EF.Functions.ILike(p.Title, pattern) ||
                         EF.Functions.ILike(p.Content, pattern)));

        if (!canViewPrivate)
            query = query.Where(p => p.Visibility == WikiVisibility.Public);

        var pages = await query.Take(MaxResults).ToListAsync(cancellationToken);

        return pages
            .Select(p => Result(
                p.Id,
                "WikiPage",
                p.Title,
                p.Content,
                $"/api/wiki-pages/{p.Id}",
                p.Visibility == WikiVisibility.MastersOnly))
            .ToList();
    }

    private async Task<IReadOnlyList<SearchResultResponse>> SearchWorldLibraryItemsAsync(
        Guid campaignId, string pattern, bool canViewPrivate, CancellationToken cancellationToken)
    {
        var query = _context.WorldLibraryItems
            .AsNoTracking()
            .Where(i => i.CampaignId == campaignId &&
                        (EF.Functions.ILike(i.Title, pattern) ||
                         (i.Description != null && EF.Functions.ILike(i.Description, pattern)) ||
                         (i.RulesText != null && EF.Functions.ILike(i.RulesText, pattern)) ||
                         (i.Notes != null && EF.Functions.ILike(i.Notes, pattern))));

        if (!canViewPrivate)
            query = query.Where(i => i.Visibility == WorldLibraryVisibility.Public);

        var items = await query.Take(MaxResults).ToListAsync(cancellationToken);

        return items
            .Select(i => Result(
                i.Id,
                "WorldLibraryItem",
                i.Title,
                i.Description,
                $"/api/world-library/{i.Id}",
                i.Visibility == WorldLibraryVisibility.MastersOnly))
            .ToList();
    }

    public async Task<IReadOnlyList<SearchResultResponse>> SearchCharacterAsync(
        Guid characterId,
        string? term,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await _context.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");

        Workspace? workspace = null;
        if (character.CampaignId is { } campaignId)
        {
            var campaign = await _context.Campaigns
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken)
                ?? throw new KeyNotFoundException("Campaign not found.");

            workspace = await _context.Workspaces
                .AsNoTracking()
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == campaign.WorkspaceId, cancellationToken)
                ?? throw new KeyNotFoundException("Workspace not found.");
        }

        EnsureCanViewCharacter(workspace, requestingUserId, character);

        var normalizedTerm = term?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
            return [];

        var pattern = $"%{normalizedTerm}%";

        var tabs = await _context.CharacterTabs
            .AsNoTracking()
            .Where(t => t.CharacterId == characterId)
            .ToListAsync(cancellationToken);

        var tabIds = tabs.Select(t => t.Id).ToList();
        var tabNamesById = tabs.ToDictionary(t => t.Id, t => t.Name);

        var blocks = await _context.CharacterTabBlocks
            .AsNoTracking()
            .Where(b => tabIds.Contains(b.CharacterTabId) &&
                        ((b.Title != null && EF.Functions.ILike(b.Title, pattern)) ||
                         (b.Content != null && EF.Functions.ILike(b.Content, pattern))))
            .Take(MaxResults)
            .ToListAsync(cancellationToken);

        return blocks
            .Select(b =>
            {
                var tabName = tabNamesById.GetValueOrDefault(b.CharacterTabId, "?");
                var title = string.IsNullOrWhiteSpace(b.Title) ? tabName : b.Title!;
                var description = string.IsNullOrWhiteSpace(b.Title) ? b.Content : $"{tabName} · {b.Content}";
                return Result(b.Id, "CharacterTabBlock", title, description, $"/api/character-tab-blocks/{b.Id}", false);
            })
            .OrderBy(r => r.Title)
            .Take(MaxResults)
            .ToList();
    }

    private static void EnsureCanViewCharacter(Workspace? workspace, Guid requestingUserId, Character character)
    {
        if (workspace is null)
        {
            if (character.UserId != requestingUserId)
                throw new KeyNotFoundException("Character not found.");
            return;
        }

        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can search this character.");
    }

    private static SearchResultResponse Result(
        Guid id,
        string type,
        string title,
        string? description,
        string url,
        bool isPrivate)
    {
        return new SearchResultResponse(id.ToString(), type, title, description, url, isPrivate);
    }
}
