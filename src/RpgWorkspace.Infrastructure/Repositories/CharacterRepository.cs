using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _context;

    public CharacterRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Characters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Character>> GetAllByCampaignAsync(
        Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.Characters
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Character character, CancellationToken cancellationToken = default)
        => await _context.Characters.AddAsync(character, cancellationToken);

    public void Remove(Character character)
        => _context.Characters.Remove(character);
}
