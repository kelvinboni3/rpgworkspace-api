using RpgWorkspace.Application.DTOs.Location;

namespace RpgWorkspace.Application.Interfaces;

public interface ILocationService
{
    Task<IReadOnlyList<LocationResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<LocationResponse> GetByIdAsync(Guid locationId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<LocationResponse> CreateAsync(Guid campaignId, CreateLocationRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<LocationResponse> UpdateAsync(Guid locationId, UpdateLocationRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid locationId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
