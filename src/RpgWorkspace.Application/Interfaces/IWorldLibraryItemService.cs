using RpgWorkspace.Application.DTOs.WorldLibraryItem;

namespace RpgWorkspace.Application.Interfaces;

public interface IWorldLibraryItemService
{
    Task<IReadOnlyList<WorldLibraryItemResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WorldLibraryItemResponse> GetByIdAsync(Guid worldLibraryItemId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WorldLibraryItemResponse> CreateAsync(Guid campaignId, CreateWorldLibraryItemRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WorldLibraryItemResponse> UpdateAsync(Guid worldLibraryItemId, UpdateWorldLibraryItemRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid worldLibraryItemId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
