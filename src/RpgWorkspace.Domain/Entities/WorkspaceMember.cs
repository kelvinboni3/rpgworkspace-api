using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class WorkspaceMember : BaseEntity
{
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    // EF Core constructor
    private WorkspaceMember() { }

    internal static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        return new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role,
        };
    }

    public void UpdateRole(WorkspaceRole role)
    {
        if (role == WorkspaceRole.Owner)
            throw new InvalidOperationException("Owner role cannot be assigned through this operation.");

        Role = role;
    }
}
