using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class WorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _context;

    public WorkspaceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<Workspace?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetAllByMemberAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
        => await _context.Workspaces.AddAsync(workspace, cancellationToken);

    public void Remove(Workspace workspace)
        => _context.Workspaces.Remove(workspace);
}
