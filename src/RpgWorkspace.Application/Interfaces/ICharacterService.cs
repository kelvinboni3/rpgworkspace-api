using RpgWorkspace.Application.DTOs.Character;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterService
{
    Task<IReadOnlyList<CharacterResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> GetByIdAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> CreateAsync(Guid campaignId, CreateCharacterRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> UpdateAsync(Guid characterId, UpdateCharacterRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
