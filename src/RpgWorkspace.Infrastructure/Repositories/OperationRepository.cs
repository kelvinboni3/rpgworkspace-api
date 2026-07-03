using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class OperationRepository : IOperationRepository
{
    private readonly AppDbContext _context;

    public OperationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Operation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Operations
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Operation>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.Operations
            .Where(o => o.CharacterId == characterId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Operation operation, CancellationToken cancellationToken = default)
        => await _context.Operations.AddAsync(operation, cancellationToken);

    public void Remove(Operation operation)
        => _context.Operations.Remove(operation);
}
