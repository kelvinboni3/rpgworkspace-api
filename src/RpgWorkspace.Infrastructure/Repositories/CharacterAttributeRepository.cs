using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CharacterAttributeRepository : ICharacterAttributeRepository
{
    private readonly AppDbContext _context;

    public CharacterAttributeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CharacterAttribute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CharacterAttributes
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CharacterAttribute>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterAttributes
            .Where(a => a.CharacterId == characterId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CharacterAttribute attribute, CancellationToken cancellationToken = default)
        => await _context.CharacterAttributes.AddAsync(attribute, cancellationToken);

    public void Remove(CharacterAttribute attribute)
        => _context.CharacterAttributes.Remove(attribute);
}
