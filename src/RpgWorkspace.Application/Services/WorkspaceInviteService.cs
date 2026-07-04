using System.Security.Cryptography;
using RpgWorkspace.Application.DTOs.WorkspaceInvite;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class WorkspaceInviteService : IWorkspaceInviteService
{
    private const int ExpirationDays = 7;

    private readonly IWorkspaceInviteRepository _workspaceInviteRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceInviteService(
        IWorkspaceInviteRepository workspaceInviteRepository,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _workspaceInviteRepository = workspaceInviteRepository;
        _workspaceRepository = workspaceRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkspaceInviteResponse>> GetAllByWorkspaceAsync(
        Guid workspaceId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        var invites = await _workspaceInviteRepository.GetAllByWorkspaceAsync(workspaceId, cancellationToken);

        return invites.Select(i => ToResponse(i)).ToList();
    }

    public async Task<WorkspaceInviteResponse> CreateAsync(
        Guid workspaceId,
        CreateWorkspaceInviteRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(workspaceId, cancellationToken);
        EnsureCanInviteRole(workspace, requestingUserId, request.Role);

        var email = request.Email.Trim().ToLowerInvariant();

        var invitedUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (invitedUser is not null && workspace.IsMember(invitedUser.Id))
            throw new InvalidOperationException("User is already a workspace member.");

        var pendingInvite = await _workspaceInviteRepository.GetPendingByWorkspaceAndEmailAsync(
            workspaceId,
            email,
            cancellationToken);

        if (pendingInvite is not null)
            throw new InvalidOperationException("A pending invite already exists for this email.");

        var token = await GenerateUniqueTokenAsync(cancellationToken);
        var invite = WorkspaceInvite.Create(
            workspaceId,
            email,
            request.Role,
            token,
            requestingUserId,
            DateTime.UtcNow.AddDays(ExpirationDays));

        await _workspaceInviteRepository.AddAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(invite, includeToken: true);
    }

    public async Task<AcceptWorkspaceInviteResponse> AcceptAsync(
        string token,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var invite = await _workspaceInviteRepository.GetByTokenAsync(token, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace invite not found.");

        EnsureInviteCanBeAccepted(invite);

        var user = await _userRepository.GetByIdAsync(requestingUserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.Equals(user.Email, invite.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Authenticated user email does not match the invite email.");

        var workspace = await GetWorkspaceWithMembersOrThrowAsync(invite.WorkspaceId, cancellationToken);
        if (workspace.IsMember(requestingUserId))
            throw new InvalidOperationException("User is already a workspace member.");

        var member = workspace.AddMember(requestingUserId, invite.Role);
        await _workspaceMemberRepository.AddAsync(member, cancellationToken);
        invite.Accept(requestingUserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AcceptWorkspaceInviteResponse(
            workspace.Id.ToString(),
            workspace.Name,
            invite.Role);
    }

    public async Task DeleteAsync(
        Guid inviteId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var invite = await _workspaceInviteRepository.GetByIdAsync(inviteId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace invite not found.");

        var workspace = await GetWorkspaceWithMembersOrThrowAsync(invite.WorkspaceId, cancellationToken);
        EnsureCanInviteRole(workspace, requestingUserId, invite.Role);

        if (invite.Status != WorkspaceInviteStatus.Pending)
            throw new InvalidOperationException("Only pending invites can be canceled.");

        invite.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureCanInviteRole(Workspace workspace, Guid userId, WorkspaceRole role)
    {
        if (role == WorkspaceRole.Owner)
            throw new UnauthorizedAccessException("Owner cannot be invited.");

        if (role == WorkspaceRole.Master)
        {
            if (!workspace.IsOwner(userId))
                throw new UnauthorizedAccessException("Only Owner can invite Master.");

            return;
        }

        if (role == WorkspaceRole.Player)
        {
            if (!workspace.IsOwnerOrMaster(userId))
                throw new UnauthorizedAccessException("Only Owner or Master can invite Player.");

            return;
        }

        throw new InvalidOperationException("Invalid workspace role.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private static void EnsureInviteCanBeAccepted(WorkspaceInvite invite)
    {
        if (invite.Status == WorkspaceInviteStatus.Accepted)
            throw new InvalidOperationException("Invite has already been accepted.");

        if (invite.Status == WorkspaceInviteStatus.Canceled)
            throw new InvalidOperationException("Invite has been canceled.");

        if (invite.Status == WorkspaceInviteStatus.Expired || invite.ExpiresAt <= DateTime.UtcNow)
        {
            if (invite.Status == WorkspaceInviteStatus.Pending)
                invite.Expire();

            throw new InvalidOperationException("Invite has expired.");
        }
    }

    private async Task<string> GenerateUniqueTokenAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 5; i++)
        {
            var token = GenerateToken();
            if (!await _workspaceInviteRepository.TokenExistsAsync(token, cancellationToken))
                return token;
        }

        throw new InvalidOperationException("Could not generate a unique invite token.");
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }

    private static WorkspaceInviteResponse ToResponse(WorkspaceInvite invite, bool includeToken = false)
    {
        var token = includeToken ? invite.Token : null;
        var inviteUrl = includeToken ? $"/api/workspace-invites/{invite.Token}/accept" : null;

        return new WorkspaceInviteResponse(
            invite.Id.ToString(),
            invite.WorkspaceId.ToString(),
            invite.Email,
            invite.Role,
            invite.Status,
            invite.ExpiresAt,
            invite.CreatedAt,
            token,
            inviteUrl);
    }
}
