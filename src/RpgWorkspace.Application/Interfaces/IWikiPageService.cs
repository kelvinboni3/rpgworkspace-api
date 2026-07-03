using RpgWorkspace.Application.DTOs.WikiPage;

namespace RpgWorkspace.Application.Interfaces;

public interface IWikiPageService
{
    Task<IReadOnlyList<WikiPageResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WikiPageResponse> GetByIdAsync(Guid wikiPageId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WikiPageResponse> CreateAsync(Guid campaignId, CreateWikiPageRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<WikiPageResponse> UpdateAsync(Guid wikiPageId, UpdateWikiPageRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid wikiPageId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
