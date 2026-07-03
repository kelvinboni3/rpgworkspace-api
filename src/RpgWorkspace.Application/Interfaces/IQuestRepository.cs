using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IQuestRepository
{
    Task<Quest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quest>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(Quest quest, CancellationToken cancellationToken = default);
    void Remove(Quest quest);
}
