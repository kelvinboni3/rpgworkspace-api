using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class BookVolumeRepository : IBookVolumeRepository
{
    private readonly AppDbContext _context;

    public BookVolumeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<BookVolume?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.BookVolumes.Include(v => v.MediaAsset).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<BookVolume>> GetAllByBlockAsync(
        Guid characterTabBlockId, CancellationToken cancellationToken = default)
    {
        return await _context.BookVolumes
            .Include(v => v.MediaAsset)
            .Where(v => v.CharacterTabBlockId == characterTabBlockId)
            .OrderBy(v => v.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BookVolume volume, CancellationToken cancellationToken = default)
        => await _context.BookVolumes.AddAsync(volume, cancellationToken);

    public void Remove(BookVolume volume)
        => _context.BookVolumes.Remove(volume);
}
