using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabRepository
{
    Task<CharacterTab?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterTab>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(CharacterTab characterTab, CancellationToken cancellationToken = default);
    void Remove(CharacterTab characterTab);
}
