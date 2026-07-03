using RpgWorkspace.Application.DTOs.Dashboard;

namespace RpgWorkspace.Application.Interfaces;

public interface IDashboardService
{
    Task<CampaignDashboardResponse> GetCampaignDashboardAsync(
        Guid campaignId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<CharacterDashboardResponse> GetCharacterDashboardAsync(
        Guid characterId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
