using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Workspace : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid OwnerUserId { get; private set; }

    // Navigation
    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();
    private readonly List<WorkspaceMember> _members = [];

    public IReadOnlyCollection<Campaign> Campaigns => _campaigns.AsReadOnly();
    private readonly List<Campaign> _campaigns = [];

    // EF Core constructor
    private Workspace() { }

    public static Workspace Create(string name, string? description, Guid ownerUserId)
    {
        var workspace = new Workspace
        {
            Name = name,
            Description = description,
            OwnerUserId = ownerUserId,
        };

        workspace._members.Add(WorkspaceMember.Create(workspace.Id, ownerUserId, WorkspaceRole.Owner));

        return workspace;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        SetUpdatedAt();
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (role == WorkspaceRole.Owner)
            throw new InvalidOperationException("Owner role cannot be added through membership invitation.");

        if (IsMember(userId))
            throw new InvalidOperationException("User is already a workspace member.");

        _members.Add(WorkspaceMember.Create(Id, userId, role));
        SetUpdatedAt();
    }

    public bool IsOwner(Guid userId) => OwnerUserId == userId;

    public bool IsOwnerOrMaster(Guid userId) =>
        Members.Any(m => m.UserId == userId &&
                         (m.Role == WorkspaceRole.Owner || m.Role == WorkspaceRole.Master));

    public bool IsMember(Guid userId) =>
        Members.Any(m => m.UserId == userId);
}
