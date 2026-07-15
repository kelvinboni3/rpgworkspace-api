using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.DTOs.CharacterTabBlock;
using RpgWorkspace.Application.DTOs.Public;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Services;

/// <summary>
/// Read-only aggregation for the public (unauthenticated) character share page.
/// Reads the DbContext directly (like SearchService/DashboardService) since it needs a
/// cross-entity, AsNoTracking query and there is no user to authorize — access is granted
/// purely by possession of the share token, and only tabs flagged IsPublic are ever exposed.
/// </summary>
public sealed class PublicCharacterService : IPublicCharacterService
{
    private readonly AppDbContext _context;

    public PublicCharacterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PublicCharacterResponse> GetByTokenAsync(
        string token, CancellationToken cancellationToken = default)
    {
        var character = await GetSharedCharacterOrThrowAsync(token, cancellationToken);

        var tabs = await _context.CharacterTabs.AsNoTracking()
            .Where(t => t.CharacterId == character.Id && t.IsPublic)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        var tabIds = tabs.Select(t => t.Id).ToList();

        // Todos os blocos das abas públicas numa query; a resolução de identidade faz o
        // fix-up das navegações e monta a árvore completa (Grupo → Registro → conteúdo).
        List<CharacterTabBlock> allBlocks = tabIds.Count == 0
            ? []
            : await _context.CharacterTabBlocks.AsNoTrackingWithIdentityResolution()
                .Where(b => tabIds.Contains(b.CharacterTabId))
                .OrderBy(b => b.Order)
                .ToListAsync(cancellationToken);

        var blocksByTab = allBlocks
            .Where(b => b.ParentBlockId == null)
            .GroupBy(b => b.CharacterTabId)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Order).ToList());

        var tabResponses = tabs
            .Select(t => new PublicCharacterTabResponse(
                t.Name,
                blocksByTab.TryGetValue(t.Id, out var tabBlocks)
                    ? tabBlocks.Select(b => ToPublicResponse(b, token)).ToList()
                    : []))
            .ToList();

        return new PublicCharacterResponse(
            character.Name,
            character.Description,
            character.Race,
            character.Class,
            character.Level,
            character.Status,
            character.PortraitAssetId.HasValue ? $"/api/public/characters/{token}/portrait" : null,
            character.HpCurrent,
            character.HpMax,
            character.MpCurrent,
            character.MpMax,
            character.AccentColor,
            tabResponses);
    }

    public async Task<(byte[] Content, string ContentType)> GetPortraitAsync(
        string token, CancellationToken cancellationToken = default)
    {
        var character = await GetSharedCharacterOrThrowAsync(token, cancellationToken);

        if (!character.PortraitAssetId.HasValue)
            throw new KeyNotFoundException("Character has no portrait.");

        return await GetAssetOrThrowAsync(character.PortraitAssetId.Value, cancellationToken);
    }

    public async Task<(byte[] Content, string ContentType)> GetBlockImageAsync(
        string token, Guid blockId, CancellationToken cancellationToken = default)
    {
        var character = await GetSharedCharacterOrThrowAsync(token, cancellationToken);

        // The block must belong to a PUBLIC tab of this specific character — never leak private-tab images.
        var block = await _context.CharacterTabBlocks.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Id == blockId
                    && b.CharacterTab.CharacterId == character.Id
                    && b.CharacterTab.IsPublic,
                cancellationToken)
            ?? throw new KeyNotFoundException("Image not found.");

        if (!block.ImageAssetId.HasValue)
            throw new KeyNotFoundException("Block has no image.");

        return await GetAssetOrThrowAsync(block.ImageAssetId.Value, cancellationToken);
    }

    private async Task<Character> GetSharedCharacterOrThrowAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new KeyNotFoundException("Character not found.");

        return await _context.Characters.AsNoTracking()
            .FirstOrDefaultAsync(c => c.PublicShareToken == token, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");
    }

    private async Task<(byte[] Content, string ContentType)> GetAssetOrThrowAsync(
        Guid assetId, CancellationToken cancellationToken)
    {
        var asset = await _context.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assetId, cancellationToken)
            ?? throw new KeyNotFoundException("Asset not found.");

        return (asset.Content, asset.ContentType);
    }

    private static CharacterTabBlockResponse ToPublicResponse(CharacterTabBlock block, string token)
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
            block.ImageAssetId.HasValue ? $"/api/public/characters/{token}/blocks/{block.Id}/image" : null,
            block.Children.OrderBy(c => c.Order).Select(c => ToPublicResponse(c, token)).ToList(),
            block.CreatedAt,
            block.UpdatedAt);
    }
}
