using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IPlayerNoteRepository
{
    Task<PlayerNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerNote>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(PlayerNote playerNote, CancellationToken cancellationToken = default);
    void Remove(PlayerNote playerNote);
}
