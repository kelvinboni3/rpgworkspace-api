using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabBlockRepository
{
    Task<CharacterTabBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterTabBlock>> GetAllByTabAsync(Guid characterTabId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterTabBlock>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterTabBlock>> GetSiblingsAsync(Guid characterTabId, Guid? parentBlockId, CancellationToken cancellationToken = default);
    Task AddAsync(CharacterTabBlock block, CancellationToken cancellationToken = default);
    void Remove(CharacterTabBlock block);

    Task SyncLinksAsync(
        Guid sourceBlockId, IReadOnlyList<Guid> targetBlockIds, Guid characterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CharacterTabBlock>> GetBacklinksAsync(Guid targetBlockId, CancellationToken cancellationToken = default);
}
