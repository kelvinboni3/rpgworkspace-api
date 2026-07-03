using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class PlayerNoteRepository : IPlayerNoteRepository
{
    private readonly AppDbContext _context;

    public PlayerNoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<PlayerNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.PlayerNotes
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PlayerNote>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerNotes
            .Where(n => n.CharacterId == characterId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PlayerNote playerNote, CancellationToken cancellationToken = default)
        => await _context.PlayerNotes.AddAsync(playerNote, cancellationToken);

    public void Remove(PlayerNote playerNote)
        => _context.PlayerNotes.Remove(playerNote);
}
