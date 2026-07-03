using RpgWorkspace.Application.DTOs.Theory;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class TheoryService : ITheoryService
{
    private readonly ITheoryRepository _theoryRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TheoryService(
        ITheoryRepository theoryRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _theoryRepository = theoryRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TheoryResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewTheories(workspace, requestingUserId, character);

        var theories = await _theoryRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        var responses = new List<TheoryResponse>();
        foreach (var theory in theories)
            responses.Add(await ToResponseAsync(theory, cancellationToken));

        return responses;
    }

    public async Task<TheoryResponse> GetByIdAsync(
        Guid theoryId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var theory = await GetTheoryOrThrowAsync(theoryId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(theory.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewTheories(workspace, requestingUserId, character);

        return await ToResponseAsync(theory, cancellationToken);
    }

    public async Task<TheoryResponse> CreateAsync(
        Guid characterId, CreateTheoryRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTheories(workspace, requestingUserId, character);
        EnsureValidConfidence(request.Confidence);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        var theory = Theory.Create(
            characterId,
            request.Title,
            request.Description,
            request.Evidence,
            request.Confidence,
            request.Status);

        await _theoryRepository.AddAsync(theory, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Theory", theory.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(theory, cancellationToken);
    }

    public async Task<TheoryResponse> UpdateAsync(
        Guid theoryId, UpdateTheoryRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var theory = await GetTheoryOrThrowAsync(theoryId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(theory.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTheories(workspace, requestingUserId, character);
        EnsureValidConfidence(request.Confidence);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        theory.Update(request.Title, request.Description, request.Evidence, request.Confidence, request.Status);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Theory", theory.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(theory, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid theoryId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var theory = await GetTheoryOrThrowAsync(theoryId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(theory.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTheories(workspace, requestingUserId, character);

        _theoryRepository.Remove(theory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Theory> GetTheoryOrThrowAsync(Guid id, CancellationToken ct)
        => await _theoryRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Theory not found.");

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

    private static void EnsureValidConfidence(int confidence)
    {
        if (confidence is < 0 or > 100)
            throw new ArgumentException("Confidence must be between 0 and 100.");
    }

    private static void EnsureCanViewTheories(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Theory not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these theories.");
    }

    private static void EnsureCanManageTheories(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Theory not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private async Task<TheoryResponse> ToResponseAsync(Theory t, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Theory", t.Id, ct);
        return new TheoryResponse(
            t.Id.ToString(),
            t.CharacterId.ToString(),
            t.Title,
            t.Description,
            t.Evidence,
            t.Confidence,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
