using RpgWorkspace.Application.DTOs.Tag;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(
        ITagRepository tagRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TagResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var tags = await _tagRepository.GetAllByCampaignAsync(campaignId, cancellationToken);
        return tags.Select(ToResponse).ToList();
    }

    public async Task<TagResponse> CreateAsync(
        Guid campaignId, CreateTagRequest request, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        if (await _tagRepository.ExistsByNameAsync(campaignId, request.Name, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Tag already exists in this campaign.");

        var tag = Tag.Create(campaignId, request.Name.Trim(), request.Color);
        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tag);
    }

    public async Task<TagResponse> UpdateAsync(
        Guid tagId, UpdateTagRequest request, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tag = await GetTagOrThrowAsync(tagId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(tag.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        if (await _tagRepository.ExistsByNameAsync(tag.CampaignId, request.Name, tag.Id, cancellationToken))
            throw new InvalidOperationException("Tag already exists in this campaign.");

        tag.Update(request.Name.Trim(), request.Color);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tag);
    }

    public async Task DeleteAsync(Guid tagId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tag = await GetTagOrThrowAsync(tagId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(tag.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _tagRepository.Remove(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Tag> GetTagOrThrowAsync(Guid id, CancellationToken ct)
        => await _tagRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Tag not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Tag not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private static TagResponse ToResponse(Tag tag) =>
        new(tag.Id.ToString(), tag.CampaignId.ToString(), tag.Name, tag.Color);
}
