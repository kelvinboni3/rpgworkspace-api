namespace RpgWorkspace.Application.DTOs.Workspace;

public sealed record WorkspaceResponse(
    string Id,
    string Name,
    string? Description,
    string OwnerUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
