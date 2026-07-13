using RpgWorkspace.Application.DTOs.Search;

namespace RpgWorkspace.Application.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<SearchResultResponse>> SearchAsync(
        Guid campaignId,
        string? term,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchResultResponse>> SearchCharacterAsync(
        Guid characterId,
        string? term,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
