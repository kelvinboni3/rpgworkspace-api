using RpgWorkspace.Application.DTOs.WorkspaceMember;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class WorkspaceMemberService : IWorkspaceMemberService
{
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceMemberService(
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkspaceMemberResponse>> GetAllByWorkspaceAsync(
        Guid workspaceId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var members = await _workspaceMemberRepository.GetAllByWorkspaceWithUsersAsync(workspaceId, cancellationToken);

        return members.Select(ToResponse).ToList();
    }

    public async Task<WorkspaceMemberResponse> UpdateRoleAsync(
        Guid workspaceId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsOwner(workspace, requestingUserId);
        EnsureAllowedRole(request.Role);

        var member = await _workspaceMemberRepository.GetByIdWithUserAsync(memberId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace member not found.");

        EnsureMemberBelongsToWorkspace(member, workspaceId);
        await EnsureChangingOwnerDoesNotLeaveWorkspaceWithoutOwnerAsync(member, requestingUserId, cancellationToken);

        member.UpdateRole(request.Role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(member);
    }

    public async Task DeleteAsync(
        Guid workspaceId,
        Guid memberId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsOwner(workspace, requestingUserId);

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace member not found.");

        EnsureMemberBelongsToWorkspace(member, workspaceId);
        await EnsureRemovingOwnerDoesNotLeaveWorkspaceWithoutOwnerAsync(member, requestingUserId, cancellationToken);

        _workspaceMemberRepository.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Workspace not found.");
    }

    private static void EnsureIsOwner(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwner(userId))
            throw new UnauthorizedAccessException("Only Owner can perform this action.");
    }

    private static void EnsureAllowedRole(WorkspaceRole role)
    {
        if (role is WorkspaceRole.Master or WorkspaceRole.Player)
            return;

        throw new InvalidOperationException("Only Master or Player roles can be assigned.");
    }

    private static void EnsureMemberBelongsToWorkspace(WorkspaceMember member, Guid workspaceId)
    {
        if (member.WorkspaceId != workspaceId)
            throw new KeyNotFoundException("Workspace member not found.");
    }

    private async Task EnsureChangingOwnerDoesNotLeaveWorkspaceWithoutOwnerAsync(
        WorkspaceMember member,
        Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        if (member.Role != WorkspaceRole.Owner)
            return;

        var ownersCount = await _workspaceMemberRepository.CountOwnersAsync(member.WorkspaceId, cancellationToken);
        if (ownersCount <= 1)
            throw new InvalidOperationException("Cannot change the last Owner role.");

        if (member.UserId == requestingUserId && ownersCount <= 1)
            throw new InvalidOperationException("Cannot change your own role if it leaves the workspace without an Owner.");
    }

    private async Task EnsureRemovingOwnerDoesNotLeaveWorkspaceWithoutOwnerAsync(
        WorkspaceMember member,
        Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        if (member.Role != WorkspaceRole.Owner)
            return;

        var ownersCount = await _workspaceMemberRepository.CountOwnersAsync(member.WorkspaceId, cancellationToken);
        if (ownersCount <= 1)
            throw new InvalidOperationException("Cannot remove the last Owner from the workspace.");

        if (member.UserId == requestingUserId && ownersCount <= 1)
            throw new InvalidOperationException("Cannot remove yourself if you are the last Owner.");
    }

    private static WorkspaceMemberResponse ToResponse(WorkspaceMember member)
    {
        var user = member.User
            ?? throw new InvalidOperationException("Workspace member user was not loaded.");

        return new WorkspaceMemberResponse(
            member.Id.ToString(),
            member.WorkspaceId.ToString(),
            member.UserId.ToString(),
            user.Name,
            user.Email,
            member.Role,
            member.CreatedAt);
    }
}
