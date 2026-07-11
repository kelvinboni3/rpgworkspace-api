using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class MediaAssetRepository : IMediaAssetRepository
{
    private readonly AppDbContext _context;

    public MediaAssetRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.MediaAssets.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default)
        => await _context.MediaAssets.AddAsync(asset, cancellationToken);

    public void Remove(MediaAsset asset)
        => _context.MediaAssets.Remove(asset);
}
