using RpgWorkspace.Application.DTOs.Campaign;

namespace RpgWorkspace.Application.Interfaces;

public interface ICampaignService
{
    Task<IReadOnlyList<CampaignResponse>> GetAllByWorkspaceAsync(Guid workspaceId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CampaignResponse> GetByIdAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CampaignResponse> CreateAsync(Guid workspaceId, CreateCampaignRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CampaignResponse> UpdateAsync(Guid campaignId, UpdateCampaignRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
