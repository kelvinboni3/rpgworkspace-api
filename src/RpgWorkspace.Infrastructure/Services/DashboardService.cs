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

        var lastPlayerNote = await _context.PlayerNotes
            .AsNoTracking()
            .Where(n => n.CharacterId == characterId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new CharacterDashboardLastPlayerNoteResponse(
                n.Id.ToString(),
                n.Title,
                n.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        var activeTheoriesQuery = _context.Theories
            .AsNoTracking()
            .Where(t => t.CharacterId == characterId && t.Status == TheoryStatus.Active);

        var activeOperationsQuery = _context.Operations
            .AsNoTracking()
            .Where(o => o.CharacterId == characterId &&
                        (o.Status == OperationStatus.Planned || o.Status == OperationStatus.InProgress));

        var activeTheoriesCount = await activeTheoriesQuery.CountAsync(cancellationToken);
        var activeOperationsCount = await activeOperationsQuery.CountAsync(cancellationToken);
        var importantPeopleCount = await _context.ImportantPeople
            .AsNoTracking()
            .CountAsync(p => p.CharacterId == characterId, cancellationToken);
        var narrativeItemsCount = await _context.NarrativeItems
            .AsNoTracking()
            .CountAsync(i => i.CharacterId == characterId, cancellationToken);

        var recentNotes = await _context.PlayerNotes
            .AsNoTracking()
            .Where(n => n.CharacterId == characterId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(RecentLimit)
            .Select(n => new CharacterDashboardPlayerNoteResponse(
                n.Id.ToString(),
                n.Title,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        var activeTheories = await activeTheoriesQuery
            .OrderByDescending(t => t.Confidence)
            .Take(RecentLimit)
            .Select(t => new CharacterDashboardTheoryResponse(
                t.Id.ToString(),
                t.Title,
                t.Confidence,
                t.Status))
            .ToListAsync(cancellationToken);

        var activeOperations = await activeOperationsQuery
            .OrderByDescending(o => o.CreatedAt)
            .Take(RecentLimit)
            .Select(o => new CharacterDashboardOperationResponse(
                o.Id.ToString(),
                o.Name,
                o.Status))
            .ToListAsync(cancellationToken);

        var importantPeopleHighlights = await _context.ImportantPeople
            .AsNoTracking()
            .Where(p => p.CharacterId == characterId)
            .OrderByDescending(p => p.RiskLevel)
            .ThenByDescending(p => p.TrustLevel)
            .ThenByDescending(p => p.UtilityLevel)
            .ThenBy(p => p.Name)
            .Take(RecentLimit)
            .Select(p => new CharacterDashboardImportantPersonResponse(
                p.Id.ToString(),
                p.Name,
                p.Type,
                p.TrustLevel,
                p.RiskLevel,
                p.UtilityLevel))
            .ToListAsync(cancellationToken);

        return new CharacterDashboardResponse(
            character.Id.ToString(),
            character.Name,
            campaign.Id.ToString(),
            campaign.Name,
            lastPlayerNote,
            activeTheoriesCount,
            activeOperationsCount,
            importantPeopleCount,
            narrativeItemsCount,
            recentNotes,
            activeTheories,
            activeOperations,
            importantPeopleHighlights);
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
