using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.DTOs.Dashboard;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private const int RecentLimit = 5;

    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignDashboardResponse> GetCampaignDashboardAsync(
        Guid campaignId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId, "Campaign not found.");

        var canViewPrivate = workspace.IsOwnerOrMaster(requestingUserId);
        var activeQuestStatuses = new[] { QuestStatus.NotStarted, QuestStatus.InProgress };

        var nextScheduleEvent = await _context.ScheduleEvents
            .AsNoTracking()
            .Where(e => e.CampaignId == campaignId && e.ProposedDate >= DateTime.UtcNow)
            .OrderBy(e => e.ProposedDate)
            .Select(e => new CampaignDashboardScheduleEventResponse(
                e.Id.ToString(),
                e.Title,
                e.ProposedDate,
                e.Status))
            .FirstOrDefaultAsync(cancellationToken);

        var lastSession = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.Date)
            .Select(s => new CampaignDashboardLastSessionResponse(
                s.Id.ToString(),
                s.Title,
                s.Number,
                s.Date,
                s.Summary))
            .FirstOrDefaultAsync(cancellationToken);

        var npcsQuery = _context.Npcs.AsNoTracking().Where(n => n.CampaignId == campaignId);
        var locationsQuery = _context.Locations.AsNoTracking().Where(l => l.CampaignId == campaignId);
        var questsQuery = _context.Quests.AsNoTracking().Where(q => q.CampaignId == campaignId);

        if (!canViewPrivate)
        {
            npcsQuery = npcsQuery.Where(n => !n.IsPrivate);
            locationsQuery = locationsQuery.Where(l => !l.IsPrivate);
            questsQuery = questsQuery.Where(q => !q.IsPrivate);
        }

        var activeQuestsQuery = questsQuery.Where(q => activeQuestStatuses.Contains(q.Status));

        var activeQuestsCount = await activeQuestsQuery.CountAsync(cancellationToken);
        var npcsCount = await npcsQuery.CountAsync(cancellationToken);
        var locationsCount = await locationsQuery.CountAsync(cancellationToken);
        var charactersCount = await _context.Characters
            .AsNoTracking()
            .CountAsync(c => c.CampaignId == campaignId, cancellationToken);

        var recentSessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.Number)
            .Take(RecentLimit)
            .Select(s => new CampaignDashboardSessionResponse(
                s.Id.ToString(),
                s.Title,
                s.Number,
                s.Date,
                s.Status))
            .ToListAsync(cancellationToken);

        var recentNpcs = await npcsQuery
            .OrderByDescending(n => n.CreatedAt)
            .Take(RecentLimit)
            .Select(n => new CampaignDashboardNpcResponse(
                n.Id.ToString(),
                n.Name,
                n.Status,
                n.IsPrivate))
            .ToListAsync(cancellationToken);

        var activeQuests = await activeQuestsQuery
            .OrderBy(q => q.Title)
            .Take(RecentLimit)
            .Select(q => new CampaignDashboardQuestResponse(
                q.Id.ToString(),
                q.Title,
                q.Status,
                q.IsPrivate))
            .ToListAsync(cancellationToken);

        return new CampaignDashboardResponse(
            campaign.Id.ToString(),
            campaign.Name,
            campaign.SystemName,
            nextScheduleEvent,
            lastSession,
            activeQuestsCount,
            npcsCount,
            locationsCount,
            charactersCount,
            recentSessions,
            recentNpcs,
            activeQuests);
    }

    public async Task<CharacterDashboardResponse> GetCharacterDashboardAsync(
        Guid characterId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await _context.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");

        var campaign = await GetCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureCanViewCharacterDashboard(workspace, requestingUserId, character);

        var tabs = await _context.CharacterTabs
            .AsNoTracking()
            .Where(t => t.CharacterId == characterId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        var tabIds = tabs.Select(t => t.Id).ToList();
        var tabNamesById = tabs.ToDictionary(t => t.Id, t => t.Name);

        var blockCountsByTab = await _context.CharacterTabBlocks
            .AsNoTracking()
            .Where(b => tabIds.Contains(b.CharacterTabId))
            .GroupBy(b => b.CharacterTabId)
            .Select(g => new { TabId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countByTabId = blockCountsByTab.ToDictionary(x => x.TabId, x => x.Count);

        var tabSummaries = tabs
            .Select(t => new CharacterDashboardTabSummaryResponse(
                t.Id.ToString(),
                t.Name,
                countByTabId.GetValueOrDefault(t.Id)))
            .ToList();

        var recentBlocks = await _context.CharacterTabBlocks
            .AsNoTracking()
            .Where(b => tabIds.Contains(b.CharacterTabId))
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Take(RecentLimit)
            .Select(b => new
            {
                b.Id,
                b.CharacterTabId,
                b.Type,
                b.Title,
                UpdatedAt = b.UpdatedAt ?? b.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var recentBlockResponses = recentBlocks
            .Select(b => new CharacterDashboardRecentBlockResponse(
                b.Id.ToString(),
                b.CharacterTabId.ToString(),
                tabNamesById.GetValueOrDefault(b.CharacterTabId, string.Empty),
                b.Type,
                b.Title,
                b.UpdatedAt))
            .ToList();

        return new CharacterDashboardResponse(
            character.Id.ToString(),
            character.Name,
            campaign.Id.ToString(),
            campaign.Name,
            tabSummaries,
            recentBlockResponses);
    }

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _context.Workspaces
            .AsNoTracking()
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId, string notFoundMessage)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException(notFoundMessage);
    }

    private static void EnsureCanViewCharacterDashboard(Workspace workspace, Guid userId, Character character)
    {
        EnsureIsMember(workspace, userId, "Character not found.");

        if (userId == character.UserId || workspace.IsOwnerOrMaster(userId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view this dashboard.");
    }
}
