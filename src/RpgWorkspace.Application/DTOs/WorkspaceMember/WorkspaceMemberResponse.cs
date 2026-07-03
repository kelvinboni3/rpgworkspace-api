using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorkspaceMember;

public sealed record WorkspaceMemberResponse(
    string Id,
    string WorkspaceId,
    string UserId,
    string UserName,
    string UserEmail,
    WorkspaceRole Role,
    DateTime CreatedAt
);
