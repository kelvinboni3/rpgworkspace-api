using RpgWorkspace.Application.DTOs.Campaign;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CampaignService(
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CampaignResponse>> GetAllByWorkspaceAsync(
        Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var campaigns = await _campaignRepository.GetAllByWorkspaceAsync(workspaceId, cancellationToken);
        return campaigns.Select(ToResponse).ToList();
    }

    public async Task<CampaignResponse> GetByIdAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        return ToResponse(campaign);
    }

    public async Task<CampaignResponse> CreateAsync(
        Guid workspaceId, CreateCampaignRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        var campaign = Campaign.Create(workspaceId, request.Name, request.Description, request.SystemName);

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(campaign);
    }

    public async Task<CampaignResponse> UpdateAsync(
        Guid campaignId, UpdateCampaignRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        campaign.Update(request.Name, request.Description, request.SystemName);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(campaign);
    }

    public async Task DeleteAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwner(workspace, requestingUserId);

        _campaignRepository.Remove(campaign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(
        Guid workspaceId, CancellationToken cancellationToken)
    {
        return await _workspaceRepository.GetByIdWithMembersAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");
    }

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Campaign not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private static void EnsureIsOwner(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwner(userId))
            throw new UnauthorizedAccessException("Only the Owner can delete campaigns.");
    }

    private static CampaignResponse ToResponse(Campaign c) =>
        new(c.Id.ToString(), c.WorkspaceId.ToString(), c.Name, c.Description, c.SystemName, c.CreatedAt, c.UpdatedAt);
}
