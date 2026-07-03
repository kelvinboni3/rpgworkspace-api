using RpgWorkspace.Application.DTOs.Workspace;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceService(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkspaceResponse>> GetAllForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var workspaces = await _workspaceRepository.GetAllByMemberAsync(userId, cancellationToken);
        return workspaces.Select(ToResponse).ToList();
    }

    public async Task<WorkspaceResponse> GetByIdAsync(
        Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdWithMembersAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");

        EnsureIsMember(workspace, requestingUserId);

        return ToResponse(workspace);
    }

    public async Task<WorkspaceResponse> CreateAsync(
        CreateWorkspaceRequest request, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var workspace = Workspace.Create(request.Name, request.Description, ownerUserId);

        await _workspaceRepository.AddAsync(workspace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(workspace);
    }

    public async Task<WorkspaceResponse> UpdateAsync(
        Guid workspaceId, UpdateWorkspaceRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");

        EnsureIsOwner(workspace, requestingUserId);

        workspace.Update(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(workspace);
    }

    public async Task DeleteAsync(
        Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");

        EnsureIsOwner(workspace, requestingUserId);

        _workspaceRepository.Remove(workspace);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void EnsureIsOwner(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwner(userId))
            throw new UnauthorizedAccessException("Only the owner can perform this action.");
    }

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        var isMember = workspace.Members.Any(m => m.UserId == userId);
        if (!isMember)
            throw new KeyNotFoundException("Workspace not found.");
    }

    private static WorkspaceResponse ToResponse(Workspace w) =>
        new(w.Id.ToString(), w.Name, w.Description, w.OwnerUserId.ToString(), w.CreatedAt, w.UpdatedAt);
}
