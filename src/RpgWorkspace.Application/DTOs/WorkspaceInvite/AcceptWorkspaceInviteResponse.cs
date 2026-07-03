using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorkspaceInvite;

public sealed record AcceptWorkspaceInviteResponse(
    string WorkspaceId,
    string WorkspaceName,
    WorkspaceRole Role
);
