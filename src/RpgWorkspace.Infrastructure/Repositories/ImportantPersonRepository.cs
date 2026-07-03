using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class ImportantPersonRepository : IImportantPersonRepository
{
    private readonly AppDbContext _context;

    public ImportantPersonRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ImportantPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.ImportantPeople
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ImportantPerson>> GetAllByCharacterAsync(
        Guid characterId, CancellationToken cancellationToken = default)
    {
        return await _context.ImportantPeople
            .Where(p => p.CharacterId == characterId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ImportantPerson importantPerson, CancellationToken cancellationToken = default)
        => await _context.ImportantPeople.AddAsync(importantPerson, cancellationToken);

    public void Remove(ImportantPerson importantPerson)
        => _context.ImportantPeople.Remove(importantPerson);
}
