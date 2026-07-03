using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorkspaceInviteRepository
{
    Task<WorkspaceInvite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceInvite>> GetAllByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceInvite?> GetPendingByWorkspaceAndEmailAsync(Guid workspaceId, string email, CancellationToken cancellationToken = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(WorkspaceInvite invite, CancellationToken cancellationToken = default);
}
