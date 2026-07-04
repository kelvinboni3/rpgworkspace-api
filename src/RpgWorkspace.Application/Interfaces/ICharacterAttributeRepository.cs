using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterAttributeRepository
{
    Task<CharacterAttribute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterAttribute>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(CharacterAttribute attribute, CancellationToken cancellationToken = default);
    void Remove(CharacterAttribute attribute);
}
