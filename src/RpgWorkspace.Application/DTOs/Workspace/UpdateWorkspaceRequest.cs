using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Workspace;

public sealed record UpdateWorkspaceRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);
