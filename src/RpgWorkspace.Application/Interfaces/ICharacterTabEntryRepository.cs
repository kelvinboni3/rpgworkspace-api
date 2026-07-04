using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabEntryRepository
{
    Task<CharacterTabEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterTabEntry>> GetAllByTabAsync(Guid characterTabId, CancellationToken cancellationToken = default);
    Task AddAsync(CharacterTabEntry entry, CancellationToken cancellationToken = default);
    void Remove(CharacterTabEntry entry);
}
