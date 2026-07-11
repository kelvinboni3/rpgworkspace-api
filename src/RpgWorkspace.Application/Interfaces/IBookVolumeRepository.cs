using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IBookVolumeRepository
{
    Task<BookVolume?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookVolume>> GetAllByBlockAsync(Guid characterTabBlockId, CancellationToken cancellationToken = default);

    Task AddAsync(BookVolume volume, CancellationToken cancellationToken = default);

    void Remove(BookVolume volume);
}
