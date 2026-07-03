using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IWikiPageRepository
{
    Task<WikiPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WikiPage>> GetAllByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(WikiPage wikiPage, CancellationToken cancellationToken = default);
    void Remove(WikiPage wikiPage);
}
