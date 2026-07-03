using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorkspaceInvite;

public sealed record CreateWorkspaceInviteRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required] WorkspaceRole Role
);
