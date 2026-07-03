using RpgWorkspace.Application.DTOs.WorkspaceInvite;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorkspaceInviteService
{
    Task<IReadOnlyList<WorkspaceInviteResponse>> GetAllByWorkspaceAsync(
        Guid workspaceId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<WorkspaceInviteResponse> CreateAsync(
        Guid workspaceId,
        CreateWorkspaceInviteRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<AcceptWorkspaceInviteResponse> AcceptAsync(
        string token,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid inviteId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
