using RpgWorkspace.Application.DTOs.NoteStructuring;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class NoteStructuringService : INoteStructuringService
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
    private readonly INoteStructuringGateway _noteStructuringGateway;
    private readonly ISubscriptionService _subscriptionService;

    public NoteStructuringService(
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterTabBlockRepository characterTabBlockRepository,
        INoteStructuringGateway noteStructuringGateway,
        ISubscriptionService subscriptionService)
    {
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _characterTabRepository = characterTabRepository;
        _characterTabBlockRepository = characterTabBlockRepository;
        _noteStructuringGateway = noteStructuringGateway;
        _subscriptionService = subscriptionService;
    }

    private delegate Task<NoteStructuringResult> GatewayCall(
        string text,
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        CancellationToken cancellationToken);

    public Task<StructureNoteResponse> StructureNoteAsync(
        Guid characterId,
        StructureNoteRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
        => ProcessAsync(characterId, request.NoteText, requestingUserId, _noteStructuringGateway.StructureAsync, cancellationToken);

    public Task<StructureNoteResponse> ImportSheetAsync(
        Guid characterId,
        ImportSheetRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
        => ProcessAsync(characterId, request.SheetText, requestingUserId, _noteStructuringGateway.ImportSheetAsync, cancellationToken);

    private async Task<StructureNoteResponse> ProcessAsync(
        Guid characterId,
        string text,
        Guid requestingUserId,
        GatewayCall gatewayCall,
        CancellationToken cancellationToken)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await CharacterAuthorizationHelper.ResolveWorkspaceAsync(
            _campaignRepository, _workspaceRepository, character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");
        await _subscriptionService.EnsureAiAccessAsync(requestingUserId, cancellationToken);

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var existingTabs = tabs.Select(t => (t.Id, t.Name)).ToList();
        var validTabIds = existingTabs.Select(t => t.Id).ToHashSet();
        var tabNamesById = tabs.ToDictionary(t => t.Id, t => t.Name);

        var allBlocks = await _characterTabBlockRepository.GetAllByCharacterAsync(characterId, cancellationToken);
        var relevantBlocks = allBlocks.Where(b => AllowedBlockTypes.Contains(b.Type)).ToList();
        var blocksById = relevantBlocks.ToDictionary(b => b.Id);

        var existingBlocks = relevantBlocks
            .Select(b => new ExistingBlockContext(
                b.Id, b.CharacterTabId, tabNamesById.GetValueOrDefault(b.CharacterTabId, "?"),
                b.Type, b.Title, b.Content, b.PayloadJson))
            .ToList();

        var characterContext = new CharacterContext(
            character.Name, character.Race, character.Class, character.Level, character.Description);

        var result = await gatewayCall(text, characterContext, existingTabs, existingBlocks, cancellationToken);

        var validated = result.Suggestions
            .Where(s => AllowedBlockTypes.Contains(s.Type))
            .Select(s => Resolve(s, blocksById, tabNamesById, validTabIds))
            .Where(s => s is not null)
            .Select(s => s!)
            .ToList();

        return new StructureNoteResponse(result.Summary, validated);
    }

    private static SuggestedBlockDto? Resolve(
        SuggestedBlockDto suggestion,
        IReadOnlyDictionary<Guid, CharacterTabBlock> blocksById,
        IReadOnlyDictionary<Guid, string> tabNamesById,
        HashSet<Guid> validTabIds)
    {
        // Never trust the model's own targetBlockId/targetTabId/label — re-derive everything
        // from the character's real tabs/blocks before it's allowed to reach the frontend.
        if (suggestion.TargetBlockId is { } blockId && blocksById.TryGetValue(blockId, out var block))
        {
            var tabName = tabNamesById.GetValueOrDefault(block.CharacterTabId, "?");
            var label = $"{tabName} · {(string.IsNullOrWhiteSpace(block.Title) ? "(sem título)" : block.Title)}";

            return suggestion with
            {
                TargetBlockId = block.Id,
                TargetBlockLabel = label,
                TargetTabId = block.CharacterTabId,
                SuggestedNewTabName = null,
                Type = block.Type,
            };
        }

        if (suggestion.TargetTabId is { } tabId && !validTabIds.Contains(tabId))
            return null;

        return suggestion with { TargetBlockId = null, TargetBlockLabel = null };
    }

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

}
