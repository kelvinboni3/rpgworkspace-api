using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid campaignId, string name, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    void Remove(Tag tag);
    Task<IReadOnlyList<Tag>> GetTagsForEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task ReplaceTagsAsync(string entityType, Guid entityId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default);
}
