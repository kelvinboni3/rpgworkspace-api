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
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab not found.");

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        return tabs.Select(ToResponse).ToList();
    }

    public async Task<CharacterTabResponse> GetByIdAsync(
        Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab not found.");

        return ToResponse(tab);
    }

    public async Task<CharacterTabResponse> CreateAsync(
        Guid characterId, CreateCharacterTabRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab not found.");

        var existingTabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var nextOrder = existingTabs.Count == 0 ? 0 : existingTabs.Max(t => t.Order) + 1;
        var tab = CharacterTab.Create(characterId, request.Name, nextOrder);

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
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab not found.");

        tab.Update(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tab);
    }

    public async Task<CharacterTabResponse> SetVisibilityAsync(
        Guid tabId, bool isPublic, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab not found.");

        tab.SetPublic(isPublic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(tab);
    }

    public async Task DeleteAsync(
        Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(tabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab not found.");

        _characterTabRepository.Remove(tab);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CharacterTab> GetCharacterTabOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab not found.");

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private Task<Workspace?> ResolveWorkspaceAsync(Guid? campaignId, CancellationToken ct)
        => CharacterAuthorizationHelper.ResolveWorkspaceAsync(_campaignRepository, _workspaceRepository, campaignId, ct);

    private static CharacterTabResponse ToResponse(CharacterTab tab)
    {
        return new CharacterTabResponse(
            tab.Id.ToString(),
            tab.CharacterId.ToString(),
            tab.Name,
            tab.Order,
            tab.IsPublic,
            tab.CreatedAt,
            tab.UpdatedAt);
    }
}
