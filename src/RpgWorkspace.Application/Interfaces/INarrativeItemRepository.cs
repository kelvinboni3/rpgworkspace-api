using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface INarrativeItemRepository
{
    Task<NarrativeItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NarrativeItem>> GetAllByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task AddAsync(NarrativeItem narrativeItem, CancellationToken cancellationToken = default);
    void Remove(NarrativeItem narrativeItem);
}
