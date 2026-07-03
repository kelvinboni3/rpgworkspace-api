using RpgWorkspace.Application.DTOs.ImportantPerson;

namespace RpgWorkspace.Application.Interfaces;

public interface IImportantPersonService
{
    Task<IReadOnlyList<ImportantPersonResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ImportantPersonResponse> GetByIdAsync(Guid importantPersonId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ImportantPersonResponse> CreateAsync(Guid characterId, CreateImportantPersonRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<ImportantPersonResponse> UpdateAsync(Guid importantPersonId, UpdateImportantPersonRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid importantPersonId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
