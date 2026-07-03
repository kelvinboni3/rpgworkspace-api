using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IImportantPersonRepository
{
    Task<ImportantPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportantPerson>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(ImportantPerson importantPerson, CancellationToken cancellationToken = default);
    void Remove(ImportantPerson importantPerson);
}
