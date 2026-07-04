using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CharacterTabEntryRepository : ICharacterTabEntryRepository
{
    private readonly AppDbContext _context;

    public CharacterTabEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CharacterTabEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CharacterTabEntries
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CharacterTabEntry>> GetAllByTabAsync(
        Guid characterTabId, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterTabEntries
            .Where(e => e.CharacterTabId == characterTabId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CharacterTabEntry entry, CancellationToken cancellationToken = default)
        => await _context.CharacterTabEntries.AddAsync(entry, cancellationToken);

    public void Remove(CharacterTabEntry entry)
        => _context.CharacterTabEntries.Remove(entry);
}
