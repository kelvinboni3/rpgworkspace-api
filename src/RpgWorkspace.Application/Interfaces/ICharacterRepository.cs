using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Character>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Character>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountSoloByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Character character, CancellationToken cancellationToken = default);
    void Remove(Character character);
}
