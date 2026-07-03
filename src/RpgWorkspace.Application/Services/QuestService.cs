using RpgWorkspace.Application.DTOs.Quest;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuestService(
        IQuestRepository questRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _questRepository = questRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<QuestResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var isPrivileged = workspace.IsOwnerOrMaster(requestingUserId);

        var quests = await _questRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        var responses = new List<QuestResponse>();
        foreach (var quest in quests.Where(q => !q.IsPrivate || isPrivileged))
            responses.Add(await ToResponseAsync(quest, cancellationToken));

        return responses;
    }

    public async Task<QuestResponse> GetByIdAsync(
        Guid questId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var quest = await GetQuestOrThrowAsync(questId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(quest.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        if (quest.IsPrivate && !workspace.IsOwnerOrMaster(requestingUserId))
            throw new KeyNotFoundException("Quest not found.");

        return await ToResponseAsync(quest, cancellationToken);
    }

    public async Task<QuestResponse> CreateAsync(
        Guid campaignId, CreateQuestRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, campaignId, request.TagIds, cancellationToken);

        var quest = Quest.Create(campaignId, request.Title, request.Description,
                                 request.Status, request.Reward, request.IsPrivate);

        await _questRepository.AddAsync(quest, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Quest", quest.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(quest, cancellationToken);
    }

    public async Task<QuestResponse> UpdateAsync(
        Guid questId, UpdateQuestRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var quest = await GetQuestOrThrowAsync(questId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(quest.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, quest.CampaignId, request.TagIds, cancellationToken);

        quest.Update(request.Title, request.Description, request.Status, request.Reward, request.IsPrivate);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Quest", quest.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(quest, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid questId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var quest = await GetQuestOrThrowAsync(questId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(quest.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _questRepository.Remove(quest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Quest> GetQuestOrThrowAsync(Guid id, CancellationToken ct)
        => await _questRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Quest not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Quest not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private async Task<QuestResponse> ToResponseAsync(Quest q, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Quest", q.Id, ct);
        return new QuestResponse(q.Id.ToString(), q.CampaignId.ToString(), q.Title, q.Description,
            q.Status, q.Reward, q.IsPrivate, q.CreatedAt, q.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
