using RpgWorkspace.Application.DTOs.Quest;

namespace RpgWorkspace.Application.Interfaces;

public interface IQuestService
{
    Task<IReadOnlyList<QuestResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<QuestResponse> GetByIdAsync(Guid questId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<QuestResponse> CreateAsync(Guid campaignId, CreateQuestRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<QuestResponse> UpdateAsync(Guid questId, UpdateQuestRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid questId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
