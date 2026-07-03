using RpgWorkspace.Application.DTOs.Operation;

namespace RpgWorkspace.Application.Interfaces;

public interface IOperationService
{
    Task<IReadOnlyList<OperationResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> GetByIdAsync(Guid operationId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> CreateAsync(Guid characterId, CreateOperationRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> UpdateAsync(Guid operationId, UpdateOperationRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid operationId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
