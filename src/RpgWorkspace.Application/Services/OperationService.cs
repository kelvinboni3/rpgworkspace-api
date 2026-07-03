using RpgWorkspace.Application.DTOs.Operation;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class OperationService : IOperationService
{
    private readonly IOperationRepository _operationRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OperationService(
        IOperationRepository operationRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _operationRepository = operationRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<OperationResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewOperations(workspace, requestingUserId, character);

        var operations = await _operationRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        var responses = new List<OperationResponse>();
        foreach (var operation in operations)
            responses.Add(await ToResponseAsync(operation, cancellationToken));

        return responses;
    }

    public async Task<OperationResponse> GetByIdAsync(
        Guid operationId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var operation = await GetOperationOrThrowAsync(operationId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(operation.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewOperations(workspace, requestingUserId, character);

        return await ToResponseAsync(operation, cancellationToken);
    }

    public async Task<OperationResponse> CreateAsync(
        Guid characterId, CreateOperationRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageOperations(workspace, requestingUserId, character);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        var operation = Operation.Create(
            characterId,
            request.Name,
            request.Objective,
            request.Plan,
            request.RequiredResources,
            request.Risks,
            request.Status,
            request.Result);

        await _operationRepository.AddAsync(operation, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Operation", operation.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(operation, cancellationToken);
    }

    public async Task<OperationResponse> UpdateAsync(
        Guid operationId, UpdateOperationRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var operation = await GetOperationOrThrowAsync(operationId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(operation.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageOperations(workspace, requestingUserId, character);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        operation.Update(
            request.Name,
            request.Objective,
            request.Plan,
            request.RequiredResources,
            request.Risks,
            request.Status,
            request.Result);

        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("Operation", operation.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(operation, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid operationId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var operation = await GetOperationOrThrowAsync(operationId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(operation.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageOperations(workspace, requestingUserId, character);

        _operationRepository.Remove(operation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Operation> GetOperationOrThrowAsync(Guid id, CancellationToken ct)
        => await _operationRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Operation not found.");

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

    private static void EnsureCanViewOperations(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Operation not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these operations.");
    }

    private static void EnsureCanManageOperations(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Operation not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private async Task<OperationResponse> ToResponseAsync(Operation o, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("Operation", o.Id, ct);
        return new OperationResponse(
            o.Id.ToString(),
            o.CharacterId.ToString(),
            o.Name,
            o.Objective,
            o.Plan,
            o.RequiredResources,
            o.Risks,
            o.Status,
            o.Result,
            o.CreatedAt,
            o.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
