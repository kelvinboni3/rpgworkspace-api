using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class TheoryRepository : ITheoryRepository
{
    private readonly AppDbContext _context;

    public TheoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Theory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Theories
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Theory>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.Theories
            .Where(t => t.CharacterId == characterId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Theory theory, CancellationToken cancellationToken = default)
        => await _context.Theories.AddAsync(theory, cancellationToken);

    public void Remove(Theory theory)
        => _context.Theories.Remove(theory);
}
