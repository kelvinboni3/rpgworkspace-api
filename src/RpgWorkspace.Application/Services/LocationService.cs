using RpgWorkspace.Application.DTOs.Location;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationService(
        ILocationRepository locationRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LocationResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var isPrivileged = workspace.IsOwnerOrMaster(requestingUserId);

        var locations = await _locationRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        var responses = new List<LocationResponse>();
        foreach (var location in locations.Where(l => !l.IsPrivate || isPrivileged))
            responses.Add(await ToResponseAsync(location, cancellationToken));

        return responses;
    }

    public async Task<LocationResponse> GetByIdAsync(
        Guid locationId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var location = await GetLocationOrThrowAsync(locationId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(location.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        if (location.IsPrivate && !workspace.IsOwnerOrMaster(requestingUserId))
            throw new KeyNotFoundException("Location not found.");

        return await ToResponseAsync(location, cancellationToken);
    }

    public async Task<LocationResponse> CreateAsync(
        Guid campaignId, CreateLocationRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var location = Location.Create(
            campaignId, request.Name, request.Type,
            request.Description, request.Region,
            request.Importance, request.IsPrivate);

        await _locationRepository.AddAsync(location, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Location", location.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(location, cancellationToken);
    }

    public async Task<LocationResponse> UpdateAsync(
        Guid locationId, UpdateLocationRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var location = await GetLocationOrThrowAsync(locationId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(location.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, location.CampaignId, request.TagIds, cancellationToken);

        location.Update(request.Name, request.Type, request.Description,
                        request.Region, request.Importance, request.IsPrivate);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Location", location.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(location, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid locationId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var location = await GetLocationOrThrowAsync(locationId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(location.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _locationRepository.Remove(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Location> GetLocationOrThrowAsync(Guid id, CancellationToken ct)
        => await _locationRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Location not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Location not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<LocationResponse> ToResponseAsync(Location l, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Location", l.Id, ct);
        return new LocationResponse(l.Id.ToString(), l.CampaignId.ToString(), l.Name, l.Type,
            l.Description, l.Region, l.Importance, l.IsPrivate, l.CreatedAt, l.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
