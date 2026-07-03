using RpgWorkspace.Application.DTOs.WorldLibraryItem;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class WorldLibraryItemService : IWorldLibraryItemService
{
    private readonly IWorldLibraryItemRepository _worldLibraryItemRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorldLibraryItemService(
        IWorldLibraryItemRepository worldLibraryItemRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _worldLibraryItemRepository = worldLibraryItemRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorldLibraryItemResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var canViewMastersOnly = workspace.IsOwnerOrMaster(requestingUserId);
        var items = await _worldLibraryItemRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        var responses = new List<WorldLibraryItemResponse>();
        foreach (var item in items.Where(i => i.Visibility == WorldLibraryVisibility.Public || canViewMastersOnly))
            responses.Add(await ToResponseAsync(item, cancellationToken));

        return responses;
    }

    public async Task<WorldLibraryItemResponse> GetByIdAsync(
        Guid worldLibraryItemId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var item = await GetWorldLibraryItemOrThrowAsync(worldLibraryItemId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(item.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureCanViewItem(workspace, requestingUserId, item);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task<WorldLibraryItemResponse> CreateAsync(
        Guid campaignId, CreateWorldLibraryItemRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var item = WorldLibraryItem.Create(
            campaignId,
            request.Title,
            request.Category,
            request.Description,
            request.RulesText,
            request.Notes,
            request.Visibility,
            requestingUserId);

        await _worldLibraryItemRepository.AddAsync(item, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("WorldLibraryItem", item.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task<WorldLibraryItemResponse> UpdateAsync(
        Guid worldLibraryItemId, UpdateWorldLibraryItemRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var item = await GetWorldLibraryItemOrThrowAsync(worldLibraryItemId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(item.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, item.CampaignId, request.TagIds, cancellationToken);

        item.Update(
            request.Title,
            request.Category,
            request.Description,
            request.RulesText,
            request.Notes,
            request.Visibility);

        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("WorldLibraryItem", item.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid worldLibraryItemId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var item = await GetWorldLibraryItemOrThrowAsync(worldLibraryItemId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(item.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _worldLibraryItemRepository.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<WorldLibraryItem> GetWorldLibraryItemOrThrowAsync(Guid id, CancellationToken ct)
        => await _worldLibraryItemRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("World library item not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("World library item not found.");
    }

    private static void EnsureCanViewItem(Workspace workspace, Guid userId, WorldLibraryItem item)
    {
        EnsureIsMember(workspace, userId);

        if (item.Visibility == WorldLibraryVisibility.Public || workspace.IsOwnerOrMaster(userId))
            return;

        throw new KeyNotFoundException("World library item not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<WorldLibraryItemResponse> ToResponseAsync(WorldLibraryItem i, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("WorldLibraryItem", i.Id, ct);
        return new WorldLibraryItemResponse(
            i.Id.ToString(),
            i.CampaignId.ToString(),
            i.Title,
            i.Category,
            i.Description,
            i.RulesText,
            i.Notes,
            i.Visibility,
            i.CreatedByUserId.ToString(),
            i.CreatedAt,
            i.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
