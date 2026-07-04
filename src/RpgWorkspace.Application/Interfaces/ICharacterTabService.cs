using RpgWorkspace.Application.DTOs.CharacterTab;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabService
{
    Task<IReadOnlyList<CharacterTabResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabResponse> GetByIdAsync(Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabResponse> CreateAsync(Guid characterId, CreateCharacterTabRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterTabResponse> UpdateAsync(Guid tabId, UpdateCharacterTabRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tabId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
