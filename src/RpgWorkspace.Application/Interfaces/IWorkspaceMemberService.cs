using RpgWorkspace.Application.DTOs.WorkspaceMember;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorkspaceMemberService
{
    Task<IReadOnlyList<WorkspaceMemberResponse>> GetAllByWorkspaceAsync(
        Guid workspaceId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<WorkspaceMemberResponse> UpdateRoleAsync(
        Guid workspaceId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid workspaceId,
        Guid memberId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
