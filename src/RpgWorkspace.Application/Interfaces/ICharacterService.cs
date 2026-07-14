using RpgWorkspace.Application.DTOs.Character;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterService
{
    Task<IReadOnlyList<CharacterResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CharacterResponse>> GetMineAsync(Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> GetByIdAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> CreateSoloAsync(Guid requestingUserId, CreateSoloCharacterRequest request, CancellationToken cancellationToken = default);
    Task<CharacterResponse> CreateAsync(Guid campaignId, CreateCharacterRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterWithAccountResponse> CreateWithAccountAsync(Guid campaignId, CreateCharacterWithAccountRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> UpdateAsync(Guid characterId, UpdateCharacterRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> UploadPortraitAsync(Guid characterId, string originalFileName, string contentType, long fileSizeBytes, Stream content, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> RemovePortraitAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType)> GetPortraitContentAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> UpdateVitalsAsync(Guid characterId, UpdateCharacterVitalsRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> UpdateAccentColorAsync(Guid characterId, UpdateCharacterAccentColorRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> EnableSharingAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterResponse> DisableSharingAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
