using RpgWorkspace.Application.DTOs.WikiPage;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class WikiPageService : IWikiPageService
{
    private readonly IWikiPageRepository _wikiPageRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WikiPageService(
        IWikiPageRepository wikiPageRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _wikiPageRepository = wikiPageRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WikiPageResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var canViewMastersOnly = workspace.IsOwnerOrMaster(requestingUserId);
        var pages = await _wikiPageRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        var responses = new List<WikiPageResponse>();
        foreach (var page in pages.Where(p => p.Visibility == WikiVisibility.Public || canViewMastersOnly))
            responses.Add(await ToResponseAsync(page, cancellationToken));

        return responses;
    }

    public async Task<WikiPageResponse> GetByIdAsync(
        Guid wikiPageId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var page = await GetWikiPageOrThrowAsync(wikiPageId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(page.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureCanViewPage(workspace, requestingUserId, page);

        return await ToResponseAsync(page, cancellationToken);
    }

    public async Task<WikiPageResponse> CreateAsync(
        Guid campaignId, CreateWikiPageRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var page = WikiPage.Create(
            campaignId,
            request.Title,
            request.Content,
            request.Visibility,
            requestingUserId);

        await _wikiPageRepository.AddAsync(page, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("WikiPage", page.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(page, cancellationToken);
    }

    public async Task<WikiPageResponse> UpdateAsync(
        Guid wikiPageId, UpdateWikiPageRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var page = await GetWikiPageOrThrowAsync(wikiPageId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(page.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, page.CampaignId, request.TagIds, cancellationToken);

        page.Update(request.Title, request.Content, request.Visibility);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("WikiPage", page.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(page, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid wikiPageId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var page = await GetWikiPageOrThrowAsync(wikiPageId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(page.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _wikiPageRepository.Remove(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<WikiPage> GetWikiPageOrThrowAsync(Guid id, CancellationToken ct)
        => await _wikiPageRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Wiki page not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Wiki page not found.");
    }

    private static void EnsureCanViewPage(Workspace workspace, Guid userId, WikiPage page)
    {
        EnsureIsMember(workspace, userId);

        if (page.Visibility == WikiVisibility.Public || workspace.IsOwnerOrMaster(userId))
            return;

        throw new KeyNotFoundException("Wiki page not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<WikiPageResponse> ToResponseAsync(WikiPage p, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("WikiPage", p.Id, ct);
        return new WikiPageResponse(
            p.Id.ToString(),
            p.CampaignId.ToString(),
            p.Title,
            p.Content,
            p.Visibility,
            p.CreatedByUserId.ToString(),
            p.CreatedAt,
            p.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
