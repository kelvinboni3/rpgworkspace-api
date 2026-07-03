using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ITheoryRepository
{
    Task<Theory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Theory>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(Theory theory, CancellationToken cancellationToken = default);
    void Remove(Theory theory);
}
