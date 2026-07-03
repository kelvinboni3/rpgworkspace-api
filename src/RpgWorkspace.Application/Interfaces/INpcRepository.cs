using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface INpcRepository
{
    Task<Npc?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Npc>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(Npc npc, CancellationToken cancellationToken = default);
    void Remove(Npc npc);
}
