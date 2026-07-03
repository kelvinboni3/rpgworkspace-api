using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorldLibraryItemRepository
{
    Task<WorldLibraryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorldLibraryItem>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(WorldLibraryItem worldLibraryItem, CancellationToken cancellationToken = default);
    void Remove(WorldLibraryItem worldLibraryItem);
}
