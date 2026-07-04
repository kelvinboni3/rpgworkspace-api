using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class WorkspaceMemberRepository : IWorkspaceMemberRepository
{
    private readonly AppDbContext _context;

    public WorkspaceMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<WorkspaceMember?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.WorkspaceMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkspaceMember>> GetAllByWorkspaceWithUsersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .Include(m => m.User)
            .Where(m => m.WorkspaceId == workspaceId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.User.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountOwnersAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => _context.WorkspaceMembers
            .CountAsync(m => m.WorkspaceId == workspaceId && m.Role == WorkspaceRole.Owner, cancellationToken);

    public async Task AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
        => await _context.WorkspaceMembers.AddAsync(member, cancellationToken);

    public void Remove(WorkspaceMember member)
        => _context.WorkspaceMembers.Remove(member);
}
