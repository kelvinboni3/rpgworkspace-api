using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorkspaceMember;

public sealed record UpdateWorkspaceMemberRoleRequest(
    [Required] WorkspaceRole Role
);
