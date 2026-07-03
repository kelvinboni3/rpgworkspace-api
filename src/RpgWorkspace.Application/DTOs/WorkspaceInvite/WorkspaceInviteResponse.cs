using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorkspaceInvite;

public sealed record WorkspaceInviteResponse(
    string Id,
    string WorkspaceId,
    string Email,
    WorkspaceRole Role,
    WorkspaceInviteStatus Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string? Token = null,
    string? InviteUrl = null
);
