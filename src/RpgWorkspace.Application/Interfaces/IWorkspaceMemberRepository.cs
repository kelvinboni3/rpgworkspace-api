using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorkspaceMemberRepository
{
    Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetAllByWorkspaceWithUsersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<int> CountOwnersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    void Remove(WorkspaceMember member);
}
