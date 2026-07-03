using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class WorkspaceInvite : BaseEntity
{
    public Guid WorkspaceId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public WorkspaceInviteStatus Status { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    // Navigation
    public Workspace Workspace { get; private set; } = null!;
    public User InvitedByUser { get; private set; } = null!;
    public User? AcceptedByUser { get; private set; }

    // EF Core constructor
    private WorkspaceInvite() { }

    public static WorkspaceInvite Create(
        Guid workspaceId,
        string email,
        WorkspaceRole role,
        string token,
        Guid invitedByUserId,
        DateTime expiresAt)
    {
        return new WorkspaceInvite
        {
            WorkspaceId = workspaceId,
            Email = email.ToLowerInvariant(),
            Role = role,
            Token = token,
            Status = WorkspaceInviteStatus.Pending,
            InvitedByUserId = invitedByUserId,
            ExpiresAt = expiresAt,
        };
    }

    public void Accept(Guid acceptedByUserId, DateTime acceptedAt)
    {
        Status = WorkspaceInviteStatus.Accepted;
        AcceptedByUserId = acceptedByUserId;
        AcceptedAt = acceptedAt;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        Status = WorkspaceInviteStatus.Canceled;
        SetUpdatedAt();
    }

    public void Expire()
    {
        Status = WorkspaceInviteStatus.Expired;
        SetUpdatedAt();
    }
}
