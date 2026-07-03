using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.Interfaces;

public interface ITagService
{
    Task<IReadOnlyList<TagResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<TagResponse> CreateAsync(Guid campaignId, CreateTagRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<TagResponse> UpdateAsync(Guid tagId, UpdateTagRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tagId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
