using RpgWorkspace.Application.DTOs.NarrativeItem;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class NarrativeItemService : INarrativeItemService
{
    private readonly INarrativeItemRepository _narrativeItemRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NarrativeItemService(
        INarrativeItemRepository narrativeItemRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        ISessionRepository sessionRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _narrativeItemRepository = narrativeItemRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _sessionRepository = sessionRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NarrativeItemResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewNarrativeItems(workspace, requestingUserId, character);

        var items = await _narrativeItemRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        var responses = new List<NarrativeItemResponse>();
        foreach (var item in items)
            responses.Add(await ToResponseAsync(item, cancellationToken));

        return responses;
    }

    public async Task<NarrativeItemResponse> GetByIdAsync(
        Guid narrativeItemId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var item = await GetNarrativeItemOrThrowAsync(narrativeItemId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(item.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewNarrativeItems(workspace, requestingUserId, character);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task<NarrativeItemResponse> CreateAsync(
        Guid characterId, CreateNarrativeItemRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNarrativeItems(workspace, requestingUserId, character);
        await EnsureSessionBelongsToCampaignAsync(request.SessionId, character.CampaignId, cancellationToken);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        var item = NarrativeItem.Create(
            characterId,
            request.Name,
            request.Description,
            request.Origin,
            request.SessionId,
            request.Importance,
            request.Notes);

        await _narrativeItemRepository.AddAsync(item, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("NarrativeItem", item.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task<NarrativeItemResponse> UpdateAsync(
        Guid narrativeItemId, UpdateNarrativeItemRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var item = await GetNarrativeItemOrThrowAsync(narrativeItemId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(item.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNarrativeItems(workspace, requestingUserId, character);
        await EnsureSessionBelongsToCampaignAsync(request.SessionId, character.CampaignId, cancellationToken);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        item.Update(request.Name, request.Description, request.Origin, request.SessionId, request.Importance, request.Notes);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("NarrativeItem", item.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid narrativeItemId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var item = await GetNarrativeItemOrThrowAsync(narrativeItemId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(item.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNarrativeItems(workspace, requestingUserId, character);

        _narrativeItemRepository.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<NarrativeItem> GetNarrativeItemOrThrowAsync(Guid id, CancellationToken ct)
        => await _narrativeItemRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Narrative item not found.");

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private async Task<Workspace> GetWorkspaceForCampaignOrThrowAsync(Guid campaignId, CancellationToken ct)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

        return await _workspaceRepository.GetByIdWithMembersAsync(campaign.WorkspaceId, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");
    }

    private async Task EnsureSessionBelongsToCampaignAsync(Guid? sessionId, Guid campaignId, CancellationToken ct)
    {
        if (sessionId is null)
            return;

        var session = await _sessionRepository.GetByIdAsync(sessionId.Value, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        if (session.CampaignId != campaignId)
            throw new ArgumentException("Session must belong to the same campaign as the character.");
    }

    private static void EnsureCanViewNarrativeItems(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Narrative item not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these narrative items.");
    }

    private static void EnsureCanManageNarrativeItems(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Narrative item not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private async Task<NarrativeItemResponse> ToResponseAsync(NarrativeItem item, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("NarrativeItem", item.Id, ct);
        return new NarrativeItemResponse(
            item.Id.ToString(),
            item.CharacterId.ToString(),
            item.Name,
            item.Description,
            item.Origin,
            item.SessionId?.ToString(),
            item.Importance,
            item.Notes,
            item.CreatedAt,
            item.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
