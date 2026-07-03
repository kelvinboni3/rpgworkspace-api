using RpgWorkspace.Application.DTOs.NarrativeItem;

namespace RpgWorkspace.Application.Interfaces;

public interface INarrativeItemService
{
    Task<IReadOnlyList<NarrativeItemResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NarrativeItemResponse> GetByIdAsync(Guid narrativeItemId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NarrativeItemResponse> CreateAsync(Guid characterId, CreateNarrativeItemRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NarrativeItemResponse> UpdateAsync(Guid narrativeItemId, UpdateNarrativeItemRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid narrativeItemId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
