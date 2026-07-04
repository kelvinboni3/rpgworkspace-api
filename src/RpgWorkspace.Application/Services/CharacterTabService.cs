using RpgWorkspace.Application.DTOs.CharacterTab;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterTabService : ICharacterTabService
{
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterTabService(
        ICharacterTabRepository characterTabRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _characterTabRepository = characterTabRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterTabResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewTabs(workspace, requestingUserId, character);

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        return tabs.Select(ToResponse).ToList();
    }

    public async Task<CharacterTabResponse> GetByIdAsync(
        Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewTabs(workspace, requestingUserId, character);

        return ToResponse(tab);
    }

    public async Task<CharacterTabResponse> CreateAsync(
        Guid characterId, CreateCharacterTabRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTabs(workspace, requestingUserId, character);

        var tab = CharacterTab.Create(characterId, request.Name);

        await _characterTabRepository.AddAsync(tab, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tab);
    }

    public async Task<CharacterTabResponse> UpdateAsync(
        Guid tabId, UpdateCharacterTabRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTabs(workspace, requestingUserId, character);

        tab.Update(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tab);
    }

    public async Task DeleteAsync(
        Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageTabs(workspace, requestingUserId, character);

        _characterTabRepository.Remove(tab);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CharacterTab> GetCharacterTabOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab not found.");

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

    private static void EnsureCanViewTabs(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character tab not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these tabs.");
    }

    private static void EnsureCanManageTabs(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character tab not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private static CharacterTabResponse ToResponse(CharacterTab tab)
    {
        return new CharacterTabResponse(
            tab.Id.ToString(),
            tab.CharacterId.ToString(),
            tab.Name,
            tab.CreatedAt,
            tab.UpdatedAt);
    }
}
