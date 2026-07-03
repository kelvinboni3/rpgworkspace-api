using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class WorkspaceInviteRepository : IWorkspaceInviteRepository
{
    private readonly AppDbContext _context;

    public WorkspaceInviteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorkspaceInvite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.WorkspaceInvites
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<WorkspaceInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => _context.WorkspaceInvites
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

    public async Task<IReadOnlyList<WorkspaceInvite>> GetAllByWorkspaceAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceInvites
            .Where(i => i.WorkspaceId == workspaceId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<WorkspaceInvite?> GetPendingByWorkspaceAndEmailAsync(
        Guid workspaceId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return _context.WorkspaceInvites
            .FirstOrDefaultAsync(
                i => i.WorkspaceId == workspaceId &&
                     i.Email == normalizedEmail &&
                     i.Status == WorkspaceInviteStatus.Pending,
                cancellationToken);
    }

    public Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default)
        => _context.WorkspaceInvites.AnyAsync(i => i.Token == token, cancellationToken);

    public async Task AddAsync(WorkspaceInvite invite, CancellationToken cancellationToken = default)
        => await _context.WorkspaceInvites.AddAsync(invite, cancellationToken);
}
