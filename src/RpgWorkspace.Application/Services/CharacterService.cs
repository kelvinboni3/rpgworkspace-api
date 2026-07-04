using RpgWorkspace.Application.DTOs.Character;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterService(
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var characters = await _characterRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        return characters.Select(ToResponse).ToList();
    }

    public async Task<CharacterResponse> GetByIdAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> CreateAsync(
        Guid campaignId, CreateCharacterRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);
        EnsureCanCreateForUser(workspace, requestingUserId, request.UserId);

        var character = Character.Create(
            campaignId,
            request.UserId,
            request.Name,
            request.Description,
            request.Race,
            request.Class,
            request.Level,
            request.Status);

        await _characterRepository.AddAsync(character, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> UpdateAsync(
        Guid characterId, UpdateCharacterRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);
        EnsureCanManageCharacter(workspace, requestingUserId, character);

        character.Update(request.Name, request.Description, request.Race, request.Class, request.Level, request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> UpdatePortraitAsync(
        Guid characterId, UpdateCharacterPortraitRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);
        EnsureCanManageCharacter(workspace, requestingUserId, character);

        character.UpdatePortrait(request.PortraitUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task DeleteAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);
        EnsureCanManageCharacter(workspace, requestingUserId, character);

        _characterRepository.Remove(character);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Character not found.");
    }

    private static void EnsureCanCreateForUser(Workspace workspace, Guid requestingUserId, Guid characterUserId)
    {
        if (!workspace.IsMember(characterUserId))
            throw new KeyNotFoundException("Workspace member not found.");

        if (requestingUserId != characterUserId && !workspace.IsOwnerOrMaster(requestingUserId))
            throw new UnauthorizedAccessException("Only Owner or Master can create characters for another user.");
    }

    private static void EnsureCanManageCharacter(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private static CharacterResponse ToResponse(Character c) =>
        new(c.Id.ToString(), c.CampaignId.ToString(), c.UserId.ToString(), c.Name, c.Description,
            c.Race, c.Class, c.Level, c.Status, c.PortraitUrl, c.CreatedAt, c.UpdatedAt);
}
