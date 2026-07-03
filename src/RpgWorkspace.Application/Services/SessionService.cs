using RpgWorkspace.Application.DTOs.Session;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(
        ISessionRepository sessionRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SessionResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var sessions = await _sessionRepository.GetAllByCampaignAsync(campaignId, cancellationToken);
        var responses = new List<SessionResponse>();
        foreach (var session in sessions)
            responses.Add(await ToResponseAsync(session, cancellationToken));

        return responses;
    }

    public async Task<SessionResponse> GetByIdAsync(
        Guid sessionId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionOrThrowAsync(sessionId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(session.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task<SessionResponse> CreateAsync(
        Guid campaignId, CreateSessionRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var session = Session.Create(
            campaignId,
            request.Title,
            request.Number,
            request.Date,
            request.Summary,
            request.Notes,
            request.Status);

        await _sessionRepository.AddAsync(session, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Session", session.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task<SessionResponse> UpdateAsync(
        Guid sessionId, UpdateSessionRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionOrThrowAsync(sessionId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(session.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, session.CampaignId, request.TagIds, cancellationToken);

        session.Update(request.Title, request.Number, request.Date, request.Summary, request.Notes, request.Status);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Session", session.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid sessionId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionOrThrowAsync(sessionId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(session.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _sessionRepository.Remove(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Session> GetSessionOrThrowAsync(Guid id, CancellationToken ct)
        => await _sessionRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Session not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Session not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<SessionResponse> ToResponseAsync(Session s, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Session", s.Id, ct);
        return new SessionResponse(s.Id.ToString(), s.CampaignId.ToString(), s.Title, s.Number, s.Date,
            s.Summary, s.Notes, s.Status, s.CreatedAt, s.UpdatedAt, TagAssociationHelper.ToResponses(tags));
    }
}
