using RpgWorkspace.Application.DTOs.Npc;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class NpcService : INpcService
{
    private readonly INpcRepository _npcRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NpcService(
        INpcRepository npcRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _npcRepository = npcRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NpcResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var isPrivileged = workspace.IsOwnerOrMaster(requestingUserId);

        var npcs = await _npcRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        var responses = new List<NpcResponse>();
        foreach (var npc in npcs.Where(n => !n.IsPrivate || isPrivileged))
            responses.Add(await ToResponseAsync(npc, cancellationToken));

        return responses;
    }

    public async Task<NpcResponse> GetByIdAsync(
        Guid npcId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var npc = await GetNpcOrThrowAsync(npcId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(npc.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        if (npc.IsPrivate && !workspace.IsOwnerOrMaster(requestingUserId))
            throw new KeyNotFoundException("Npc not found.");

        return await ToResponseAsync(npc, cancellationToken);
    }

    public async Task<NpcResponse> CreateAsync(
        Guid campaignId, CreateNpcRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var npc = Npc.Create(campaignId, request.Name, request.Description,
                             request.Status, request.IsPrivate, request.Notes);

        await _npcRepository.AddAsync(npc, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Npc", npc.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(npc, cancellationToken);
    }

    public async Task<NpcResponse> UpdateAsync(
        Guid npcId, UpdateNpcRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var npc = await GetNpcOrThrowAsync(npcId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(npc.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, npc.CampaignId, request.TagIds, cancellationToken);

        npc.Update(request.Name, request.Description, request.Status, request.IsPrivate, request.Notes);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Npc", npc.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(npc, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid npcId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var npc = await GetNpcOrThrowAsync(npcId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(npc.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _npcRepository.Remove(npc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Npc> GetNpcOrThrowAsync(Guid id, CancellationToken ct)
        => await _npcRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Npc not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Npc not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<NpcResponse> ToResponseAsync(Npc n, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Npc", n.Id, ct);
        return new NpcResponse(n.Id.ToString(), n.CampaignId.ToString(), n.Name, n.Description,
            n.Status, n.IsPrivate, n.Notes, n.CreatedAt, n.UpdatedAt, TagAssociationHelper.ToResponses(tags));
    }
}
