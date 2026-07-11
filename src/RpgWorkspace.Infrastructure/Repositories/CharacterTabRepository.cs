using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CharacterTabRepository : ICharacterTabRepository
{
    private readonly AppDbContext _context;

    public CharacterTabRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CharacterTab?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CharacterTabs
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CharacterTab>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterTabs
            .Where(t => t.CharacterId == characterId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CharacterTab characterTab, CancellationToken cancellationToken = default)
        => await _context.CharacterTabs.AddAsync(characterTab, cancellationToken);

    public void Remove(CharacterTab characterTab)
        => _context.CharacterTabs.Remove(characterTab);
}
