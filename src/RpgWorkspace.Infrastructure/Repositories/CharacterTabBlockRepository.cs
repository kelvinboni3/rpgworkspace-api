using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CharacterTabBlockRepository : ICharacterTabBlockRepository
{
    private readonly AppDbContext _context;

    public CharacterTabBlockRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CharacterTabBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CharacterTabBlocks
            .Include(b => b.Children.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CharacterTabBlock>> GetAllByTabAsync(
        Guid characterTabId, CancellationToken cancellationToken = default)
    {
        // Carrega TODOS os blocos da aba numa query rastreada: o fix-up de navegação do EF
        // monta a árvore inteira (Grupo → Registro expansível → conteúdo), sem depender de
        // uma cadeia de Include/ThenInclude com profundidade fixa.
        var all = await _context.CharacterTabBlocks
            .Where(b => b.CharacterTabId == characterTabId)
            .OrderBy(b => b.Order)
            .ToListAsync(cancellationToken);

        return all.Where(b => b.ParentBlockId == null).OrderBy(b => b.Order).ToList();
    }

    public async Task<IReadOnlyList<CharacterTabBlock>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterTabBlocks
            .Include(b => b.CharacterTab)
            .Where(b => b.CharacterTab.CharacterId == characterId)
            .OrderBy(b => b.CharacterTab.Order).ThenBy(b => b.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CharacterTabBlock>> GetSiblingsAsync(
        Guid characterTabId, Guid? parentBlockId, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterTabBlocks
            .Where(b => b.CharacterTabId == characterTabId && b.ParentBlockId == parentBlockId)
            .OrderBy(b => b.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CharacterTabBlock block, CancellationToken cancellationToken = default)
        => await _context.CharacterTabBlocks.AddAsync(block, cancellationToken);

    public void Remove(CharacterTabBlock block)
        => _context.CharacterTabBlocks.Remove(block);

    public async Task SyncLinksAsync(
        Guid sourceBlockId, IReadOnlyList<Guid> targetBlockIds, Guid characterId,
        CancellationToken cancellationToken = default)
    {
        var validTargetIds = targetBlockIds.Count == 0
            ? []
            : await _context.CharacterTabBlocks
                .Where(b => targetBlockIds.Contains(b.Id) && b.Id != sourceBlockId && b.CharacterTab.CharacterId == characterId)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

        var existingLinks = await _context.CharacterTabBlockLinks
            .Where(l => l.SourceBlockId == sourceBlockId)
            .ToListAsync(cancellationToken);

        var toRemove = existingLinks.Where(l => !validTargetIds.Contains(l.TargetBlockId));
        _context.CharacterTabBlockLinks.RemoveRange(toRemove);

        var existingTargetIds = existingLinks.Select(l => l.TargetBlockId).ToHashSet();
        foreach (var targetId in validTargetIds.Where(id => !existingTargetIds.Contains(id)))
            await _context.CharacterTabBlockLinks.AddAsync(CharacterTabBlockLink.Create(sourceBlockId, targetId), cancellationToken);
    }

    public async Task<IReadOnlyList<CharacterTabBlock>> GetBacklinksAsync(
        Guid targetBlockId, CancellationToken cancellationToken = default)
    {
        var sourceBlockIds = _context.CharacterTabBlockLinks
            .Where(l => l.TargetBlockId == targetBlockId)
            .Select(l => l.SourceBlockId);

        return await _context.CharacterTabBlocks
            .Include(b => b.CharacterTab)
            .Where(b => sourceBlockIds.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }
}
