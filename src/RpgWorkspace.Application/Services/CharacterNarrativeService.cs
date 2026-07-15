using RpgWorkspace.Application.DTOs.CharacterNarrative;
using RpgWorkspace.Application.DTOs.NoteStructuring;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterNarrativeService : ICharacterNarrativeService
{
    private static readonly HashSet<CharacterTabBlockType> AllowedBlockTypes =
    [
        CharacterTabBlockType.Text,
        CharacterTabBlockType.Quote,
        CharacterTabBlockType.Card,
        CharacterTabBlockType.Table,
        CharacterTabBlockType.Divider,
        CharacterTabBlockType.Collapse,
    ];

    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterTabBlockRepository _characterTabBlockRepository;
    private readonly ICharacterNarrativeGateway _narrativeGateway;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterNarrativeService(
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterTabBlockRepository characterTabBlockRepository,
        ICharacterNarrativeGateway narrativeGateway,
        ISubscriptionService subscriptionService,
        IUnitOfWork unitOfWork)
    {
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _characterTabRepository = characterTabRepository;
        _characterTabBlockRepository = characterTabBlockRepository;
        _narrativeGateway = narrativeGateway;
        _subscriptionService = subscriptionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CharacterRecapResponse> GenerateRecapAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var (character, characterContext, existingTabs, existingBlocks) =
            await LoadContextAsync(characterId, requestingUserId, cancellationToken);

        var text = await _narrativeGateway.GenerateRecapAsync(
            characterContext, existingTabs, existingBlocks, requestingUserId, cancellationToken);

        var generatedAt = DateTime.UtcNow;
        character.SetRecap(text, generatedAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CharacterRecapResponse(text, generatedAt);
    }

    public async Task<CharacterRetrospectiveResponse> GenerateRetrospectiveAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var (character, characterContext, existingTabs, existingBlocks) =
            await LoadContextAsync(characterId, requestingUserId, cancellationToken);

        var text = await _narrativeGateway.GenerateRetrospectiveAsync(
            characterContext, existingTabs, existingBlocks, requestingUserId, cancellationToken);

        character.SetRetrospective(text);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CharacterRetrospectiveResponse(text);
    }

    private static readonly TimeSpan MinimumMemoryAge = TimeSpan.FromDays(3);

    public async Task<CharacterMemoryResponse?> GetMemoryAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");

        var workspace = await CharacterAuthorizationHelper.ResolveWorkspaceAsync(
            _campaignRepository, _workspaceRepository, character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var tabNamesById = tabs.ToDictionary(t => t.Id, t => t.Name);

        var cutoff = DateTime.UtcNow - MinimumMemoryAge;
        var allBlocks = await _characterTabBlockRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var eligible = allBlocks
            .Where(b => !string.IsNullOrWhiteSpace(b.Content) && b.CreatedAt <= cutoff)
            .ToList();

        if (eligible.Count == 0)
            return null;

        var chosen = eligible[Random.Shared.Next(eligible.Count)];

        return new CharacterMemoryResponse(
            chosen.Id.ToString(),
            chosen.CharacterTabId.ToString(),
            tabNamesById.GetValueOrDefault(chosen.CharacterTabId, "?"),
            chosen.Title,
            chosen.Content!,
            chosen.CreatedAt);
    }

    private async Task<(
        Character Character,
        CharacterContext Context,
        IReadOnlyList<(Guid Id, string Name)> ExistingTabs,
        IReadOnlyList<ExistingBlockContext> ExistingBlocks)> LoadContextAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");

        var workspace = await CharacterAuthorizationHelper.ResolveWorkspaceAsync(
            _campaignRepository, _workspaceRepository, character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");
        await _subscriptionService.EnsureAiAccessAsync(requestingUserId, cancellationToken);

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var existingTabs = tabs.Select(t => (t.Id, t.Name)).ToList();
        var tabNamesById = tabs.ToDictionary(t => t.Id, t => t.Name);

        var allBlocks = await _characterTabBlockRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var existingBlocks = allBlocks
            .Where(b => AllowedBlockTypes.Contains(b.Type))
            .Select(b => new ExistingBlockContext(
                b.Id, b.CharacterTabId, tabNamesById.GetValueOrDefault(b.CharacterTabId, "?"),
                b.Type, b.Title, b.Content, b.PayloadJson))
            .ToList();

        var characterContext = new CharacterContext(
            character.Name, character.Race, character.Class, character.Level, character.Description);

        return (character, characterContext, existingTabs, existingBlocks);
    }
}
