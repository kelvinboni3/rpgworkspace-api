using RpgWorkspace.Application.DTOs.CharacterAttribute;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterAttributeService : ICharacterAttributeService
{
    private readonly ICharacterAttributeRepository _characterAttributeRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterAttributeService(
        ICharacterAttributeRepository characterAttributeRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _characterAttributeRepository = characterAttributeRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterAttributeResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewAttributes(workspace, requestingUserId, character);

        var attributes = await _characterAttributeRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        return attributes.Select(ToResponse).ToList();
    }

    public async Task<CharacterAttributeResponse> GetByIdAsync(
        Guid attributeId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeOrThrowAsync(attributeId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(attribute.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewAttributes(workspace, requestingUserId, character);

        return ToResponse(attribute);
    }

    public async Task<CharacterAttributeResponse> CreateAsync(
        Guid characterId, CreateCharacterAttributeRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageAttributes(workspace, requestingUserId, character);

        var attribute = CharacterAttribute.Create(characterId, request.Name, request.Value);

        await _characterAttributeRepository.AddAsync(attribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(attribute);
    }

    public async Task<CharacterAttributeResponse> UpdateAsync(
        Guid attributeId, UpdateCharacterAttributeRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeOrThrowAsync(attributeId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(attribute.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageAttributes(workspace, requestingUserId, character);

        attribute.Update(request.Name, request.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(attribute);
    }

    public async Task DeleteAsync(
        Guid attributeId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeOrThrowAsync(attributeId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(attribute.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageAttributes(workspace, requestingUserId, character);

        _characterAttributeRepository.Remove(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CharacterAttribute> GetAttributeOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterAttributeRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character attribute not found.");

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

    private static void EnsureCanViewAttributes(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character attribute not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these attributes.");
    }

    private static void EnsureCanManageAttributes(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character attribute not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private static CharacterAttributeResponse ToResponse(CharacterAttribute attribute)
    {
        return new CharacterAttributeResponse(
            attribute.Id.ToString(),
            attribute.CharacterId.ToString(),
            attribute.Name,
            attribute.Value,
            attribute.CreatedAt,
            attribute.UpdatedAt);
    }
}
