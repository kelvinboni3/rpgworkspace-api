using RpgWorkspace.Application.DTOs.CharacterTabEntry;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterTabEntryService : ICharacterTabEntryService
{
    private readonly ICharacterTabEntryRepository _characterTabEntryRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterTabEntryService(
        ICharacterTabEntryRepository characterTabEntryRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _characterTabEntryRepository = characterTabEntryRepository;
        _characterTabRepository = characterTabRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterTabEntryResponse>> GetAllByTabAsync(
        Guid characterTabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(characterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewEntries(workspace, requestingUserId, character);

        var entries = await _characterTabEntryRepository.GetAllByTabAsync(characterTabId, cancellationToken);
        return entries.Select(ToResponse).ToList();
    }

    public async Task<CharacterTabEntryResponse> GetByIdAsync(
        Guid entryId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var entry = await GetEntryOrThrowAsync(entryId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(entry.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewEntries(workspace, requestingUserId, character);

        return ToResponse(entry);
    }

    public async Task<CharacterTabEntryResponse> CreateAsync(
        Guid characterTabId, CreateCharacterTabEntryRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(characterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageEntries(workspace, requestingUserId, character);

        var entry = CharacterTabEntry.Create(characterTabId, request.Title, request.Content);

        await _characterTabEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(entry);
    }

    public async Task<CharacterTabEntryResponse> UpdateAsync(
        Guid entryId, UpdateCharacterTabEntryRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var entry = await GetEntryOrThrowAsync(entryId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(entry.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageEntries(workspace, requestingUserId, character);

        entry.Update(request.Title, request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(entry);
    }

    public async Task DeleteAsync(
        Guid entryId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var entry = await GetEntryOrThrowAsync(entryId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(entry.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageEntries(workspace, requestingUserId, character);

        _characterTabEntryRepository.Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CharacterTabEntry> GetEntryOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabEntryRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab entry not found.");

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

    private static void EnsureCanViewEntries(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character tab entry not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these entries.");
    }

    private static void EnsureCanManageEntries(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Character tab entry not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private static CharacterTabEntryResponse ToResponse(CharacterTabEntry entry)
    {
        return new CharacterTabEntryResponse(
            entry.Id.ToString(),
            entry.CharacterTabId.ToString(),
            entry.Title,
            entry.Content,
            entry.CreatedAt,
            entry.UpdatedAt);
    }
}
