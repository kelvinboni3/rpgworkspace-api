using RpgWorkspace.Application.DTOs.Workspace;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorkspaceService
{
    Task<IReadOnlyList<WorkspaceResponse>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkspaceResponse> GetByIdAsync(Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<WorkspaceResponse> UpdateAsync(Guid workspaceId, UpdateWorkspaceRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
