using RpgWorkspace.Application.DTOs.Theory;

namespace RpgWorkspace.Application.Interfaces;

public interface ITheoryService
{
    Task<IReadOnlyList<TheoryResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<TheoryResponse> GetByIdAsync(Guid theoryId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<TheoryResponse> CreateAsync(Guid characterId, CreateTheoryRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<TheoryResponse> UpdateAsync(Guid theoryId, UpdateTheoryRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid theoryId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
