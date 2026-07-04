using RpgWorkspace.Application.DTOs.CharacterTabEntry;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabEntryService
{
    Task<IReadOnlyList<CharacterTabEntryResponse>> GetAllByTabAsync(Guid characterTabId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabEntryResponse> GetByIdAsync(Guid entryId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabEntryResponse> CreateAsync(Guid characterTabId, CreateCharacterTabEntryRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabEntryResponse> UpdateAsync(Guid entryId, UpdateCharacterTabEntryRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid entryId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
