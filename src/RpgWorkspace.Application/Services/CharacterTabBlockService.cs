using System.Text.RegularExpressions;
using RpgWorkspace.Application.DTOs.CharacterTabBlock;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterTabBlockService : ICharacterTabBlockService
{
    private const long MaxImageFileSizeBytes = 5 * 1024 * 1024;

    private readonly ICharacterTabBlockRepository _characterTabBlockRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterTabBlockService(
        ICharacterTabBlockRepository characterTabBlockRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IMediaAssetRepository mediaAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _characterTabBlockRepository = characterTabBlockRepository;
        _characterTabRepository = characterTabRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterTabBlockResponse>> GetAllByTabAsync(
        Guid characterTabId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(characterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab block not found.");

        var blocks = await _characterTabBlockRepository.GetAllByTabAsync(characterTabId, cancellationToken);
        return blocks.Select(ToResponse).ToList();
    }

    public async Task<CharacterTabBlockResponse> GetByIdAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab block not found.");

        return ToResponse(block);
    }

    public async Task<CharacterTabBlockResponse> CreateAsync(
        Guid characterTabId, CreateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(characterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        var siblings = await _characterTabBlockRepository.GetSiblingsAsync(characterTabId, null, cancellationToken);
        var nextOrder = siblings.Count == 0 ? 0 : siblings.Max(b => b.Order) + 1;

        var block = CharacterTabBlock.Create(
            characterTabId, request.Type, nextOrder, request.Title, request.Meta, request.Content, request.PayloadJson,
            parentBlockId: null, accentColor: NormalizeAccentColor(request.AccentColor));

        await _characterTabBlockRepository.AddAsync(block, cancellationToken);
        await SyncBlockLinksAsync(block, character.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<CharacterTabBlockResponse> CreateChildAsync(
        Guid parentBlockId, CreateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var parent = await GetBlockOrThrowAsync(parentBlockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(parent.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        if (parent.Type is not (CharacterTabBlockType.Collapse or CharacterTabBlockType.Image or CharacterTabBlockType.Group))
            throw new ArgumentException("Only 'Grupo', 'Registro expansível' or 'Imagem' blocks can contain nested blocks.");

        if (request.Type == CharacterTabBlockType.Group)
            throw new ArgumentException("A 'Grupo' block cannot be nested inside another block.");

        // Registro expansível aninhado só dentro de Grupo (é o caso "sala com personagens").
        if (request.Type == CharacterTabBlockType.Collapse && parent.Type != CharacterTabBlockType.Group)
            throw new ArgumentException("A 'Registro expansível' block can only be nested inside a 'Grupo' block.");

        if (request.Type == CharacterTabBlockType.Image && parent.Type == CharacterTabBlockType.Image)
            throw new ArgumentException("An 'Imagem' block cannot be nested inside another one.");

        var siblings = await _characterTabBlockRepository.GetSiblingsAsync(parent.CharacterTabId, parent.Id, cancellationToken);
        var nextOrder = siblings.Count == 0 ? 0 : siblings.Max(b => b.Order) + 1;

        var block = CharacterTabBlock.Create(
            parent.CharacterTabId, request.Type, nextOrder, request.Title, request.Meta, request.Content, request.PayloadJson,
            parentBlockId: parent.Id, accentColor: NormalizeAccentColor(request.AccentColor));

        await _characterTabBlockRepository.AddAsync(block, cancellationToken);
        await SyncBlockLinksAsync(block, character.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<CharacterTabBlockResponse> SetParentAsync(
        Guid blockId, SetCharacterTabBlockParentRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        if (block.ParentBlockId == request.ParentBlockId)
            return ToResponse(block);

        if (request.ParentBlockId is Guid newParentId)
        {
            if (newParentId == block.Id)
                throw new ArgumentException("A block cannot be moved inside itself.");

            var parent = await GetBlockOrThrowAsync(newParentId, cancellationToken);

            if (parent.CharacterTabId != block.CharacterTabId)
                throw new ArgumentException("Blocks can only be moved inside a container of the same tab.");

            if (parent.ParentBlockId is not null)
                throw new ArgumentException("Blocks can only be moved inside a top-level container.");

            // Mesmas regras de aninhamento do CreateChildAsync.
            if (parent.Type is not (CharacterTabBlockType.Collapse or CharacterTabBlockType.Image or CharacterTabBlockType.Group))
                throw new ArgumentException("Only 'Grupo', 'Registro expansível' or 'Imagem' blocks can contain nested blocks.");

            if (block.Type == CharacterTabBlockType.Group)
                throw new ArgumentException("A 'Grupo' block cannot be nested inside another block.");

            if (block.Type == CharacterTabBlockType.Collapse && parent.Type != CharacterTabBlockType.Group)
                throw new ArgumentException("A 'Registro expansível' block can only be moved inside a 'Grupo' block.");

            if (block.Type == CharacterTabBlockType.Image && parent.Type == CharacterTabBlockType.Image)
                throw new ArgumentException("An 'Imagem' block cannot be nested inside another one.");

            // Um Grupo aceita blocos que já têm conteúdo aninhado (ex.: personagens com blocos
            // dentro); nos demais containers o bloco movido precisa estar "vazio" de filhos.
            if (block.Children.Count > 0 && parent.Type != CharacterTabBlockType.Group)
                throw new ArgumentException("A block that contains nested blocks can only be moved inside a 'Grupo' block.");
        }

        // Entra sempre no fim da lista de destino (topo da aba ou filhos do container).
        var newSiblings = await _characterTabBlockRepository.GetSiblingsAsync(
            block.CharacterTabId, request.ParentBlockId, cancellationToken);
        var nextOrder = newSiblings.Count == 0 ? 0 : newSiblings.Max(b => b.Order) + 1;

        block.SetParent(request.ParentBlockId, nextOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<CharacterTabBlockResponse> UpdateAsync(
        Guid blockId, UpdateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        block.Update(request.Title, request.Meta, request.Content, request.PayloadJson);
        await SyncBlockLinksAsync(block, character.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<IReadOnlyList<CharacterTabBlockResponse>> MoveAsync(
        Guid blockId, MoveCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        if (request.Direction is not ("up" or "down"))
            throw new ArgumentException("Direction must be 'up' or 'down'.");

        var siblings = (await _characterTabBlockRepository.GetSiblingsAsync(block.CharacterTabId, block.ParentBlockId, cancellationToken))
            .OrderBy(b => b.Order)
            .ToList();

        var index = siblings.FindIndex(b => b.Id == block.Id);
        var targetIndex = request.Direction == "up" ? index - 1 : index + 1;

        if (targetIndex >= 0 && targetIndex < siblings.Count)
        {
            var target = siblings[targetIndex];
            var currentOrder = block.Order;
            block.Reorder(target.Order);
            target.Reorder(currentOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return siblings.OrderBy(b => b.Order).Select(ToResponse).ToList();
    }

    public async Task<CharacterTabBlockResponse> UpdateAccentColorAsync(
        Guid blockId, UpdateCharacterTabBlockAccentColorRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        block.SetAccentColor(NormalizeAccentColor(request.AccentColor));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    private static readonly HashSet<string> AllowedAccentColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "gold", "crimson", "violet",
    };

    private static string? NormalizeAccentColor(string? accentColor)
    {
        if (string.IsNullOrWhiteSpace(accentColor))
            return null;

        if (!AllowedAccentColors.Contains(accentColor))
            throw new ArgumentException($"Invalid accent color: {accentColor}.");

        return accentColor.ToLowerInvariant();
    }

    public async Task<IReadOnlyList<CharacterTabBlockResponse>> ReorderAsync(
        Guid characterTabId, ReorderCharacterTabBlocksRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var tab = await GetCharacterTabOrThrowAsync(characterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        var siblings = await _characterTabBlockRepository.GetSiblingsAsync(characterTabId, null, cancellationToken);
        ApplyReorder(siblings, request.OrderedBlockIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return siblings.OrderBy(b => b.Order).Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<CharacterTabBlockResponse>> ReorderChildrenAsync(
        Guid parentBlockId, ReorderCharacterTabBlocksRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var parent = await GetBlockOrThrowAsync(parentBlockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(parent.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        var siblings = await _characterTabBlockRepository.GetSiblingsAsync(parent.CharacterTabId, parent.Id, cancellationToken);
        ApplyReorder(siblings, request.OrderedBlockIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return siblings.OrderBy(b => b.Order).Select(ToResponse).ToList();
    }

    private static void ApplyReorder(IReadOnlyList<CharacterTabBlock> siblings, IReadOnlyList<Guid> orderedBlockIds)
    {
        if (orderedBlockIds.Count != siblings.Count || !orderedBlockIds.ToHashSet().SetEquals(siblings.Select(b => b.Id)))
            throw new ArgumentException("Reorder list must contain exactly the current set of sibling blocks.");

        var byId = siblings.ToDictionary(b => b.Id);
        for (var i = 0; i < orderedBlockIds.Count; i++)
            byId[orderedBlockIds[i]].Reorder(i);
    }

    public async Task DeleteAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        _characterTabBlockRepository.Remove(block);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CharacterTabBlockResponse> SetImageAsync(
        Guid blockId, string originalFileName, string contentType, long fileSizeBytes, Stream content,
        Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only image files are accepted.");

        if (fileSizeBytes <= 0 || fileSizeBytes > MaxImageFileSizeBytes)
            throw new ArgumentException($"File size must be between 1 byte and {MaxImageFileSizeBytes / (1024 * 1024)} MB.");

        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);

        var previousAssetId = block.ImageAssetId;

        var asset = MediaAsset.Create(memoryStream.ToArray(), contentType, originalFileName, fileSizeBytes);
        await _mediaAssetRepository.AddAsync(asset, cancellationToken);
        block.SetImage(asset.Id);

        if (previousAssetId.HasValue)
        {
            var previousAsset = await _mediaAssetRepository.GetByIdAsync(previousAssetId.Value, cancellationToken);
            if (previousAsset is not null)
                _mediaAssetRepository.Remove(previousAsset);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<CharacterTabBlockResponse> RemoveImageAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character tab block not found.");

        if (block.ImageAssetId.HasValue)
        {
            var previousAsset = await _mediaAssetRepository.GetByIdAsync(block.ImageAssetId.Value, cancellationToken);
            if (previousAsset is not null)
                _mediaAssetRepository.Remove(previousAsset);
        }

        block.SetImage(null);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(block);
    }

    public async Task<(byte[] Content, string ContentType)> GetImageContentAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab block not found.");

        if (!block.ImageAssetId.HasValue)
            throw new KeyNotFoundException("Block has no image.");

        var asset = await _mediaAssetRepository.GetByIdAsync(block.ImageAssetId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Image not found.");

        return (asset.Content, asset.ContentType);
    }

    public async Task<IReadOnlyList<CharacterTabBlockBacklinkResponse>> GetBacklinksAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(blockId, cancellationToken);
        var tab = await GetCharacterTabOrThrowAsync(block.CharacterTabId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(tab.CharacterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character tab block not found.");

        var sourceBlocks = await _characterTabBlockRepository.GetBacklinksAsync(blockId, cancellationToken);

        return sourceBlocks
            .Select(b => new CharacterTabBlockBacklinkResponse(
                b.Id.ToString(),
                DescribeBlock(b),
                b.CharacterTabId.ToString(),
                b.CharacterTab.Name))
            .ToList();
    }

    private static string? DescribeBlock(CharacterTabBlock block)
    {
        if (!string.IsNullOrWhiteSpace(block.Title))
            return block.Title;

        if (string.IsNullOrWhiteSpace(block.Content))
            return null;

        var plain = BlockLinkPattern.Replace(block.Content, string.Empty);
        plain = plain.Replace("[", string.Empty).Replace("]", string.Empty)
            .Replace("(", string.Empty).Replace(")", string.Empty)
            .Replace("\n", " ").Trim();

        const int maxLength = 60;
        return plain.Length > maxLength ? plain[..maxLength].TrimEnd() + "…" : plain;
    }

    private static readonly Regex BlockLinkPattern = new(
        @"block:([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})",
        RegexOptions.Compiled);

    private static IReadOnlyList<Guid> ExtractLinkedBlockIds(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return [];

        var ids = new List<Guid>();
        foreach (Match match in BlockLinkPattern.Matches(content))
        {
            if (Guid.TryParse(match.Groups[1].Value, out var id) && !ids.Contains(id))
                ids.Add(id);
        }

        return ids;
    }

    private async Task SyncBlockLinksAsync(CharacterTabBlock block, Guid characterId, CancellationToken cancellationToken)
    {
        var targetIds = ExtractLinkedBlockIds(block.Content);
        await _characterTabBlockRepository.SyncLinksAsync(block.Id, targetIds, characterId, cancellationToken);
    }

    private async Task<CharacterTabBlock> GetBlockOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabBlockRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab block not found.");

    private async Task<CharacterTab> GetCharacterTabOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab not found.");

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private Task<Workspace?> ResolveWorkspaceAsync(Guid? campaignId, CancellationToken ct)
        => CharacterAuthorizationHelper.ResolveWorkspaceAsync(_campaignRepository, _workspaceRepository, campaignId, ct);

    private static CharacterTabBlockResponse ToResponse(CharacterTabBlock block)
    {
        return new CharacterTabBlockResponse(
            block.Id.ToString(),
            block.CharacterTabId.ToString(),
            block.ParentBlockId?.ToString(),
            block.Type,
            block.Order,
            block.Title,
            block.Meta,
            block.Content,
            block.PayloadJson,
            block.AccentColor,
            block.ImageAssetId.HasValue ? $"/api/character-tab-blocks/{block.Id}/image" : null,
            block.Children.OrderBy(c => c.Order).Select(ToResponse).ToList(),
            block.CreatedAt,
            block.UpdatedAt);
    }
}
