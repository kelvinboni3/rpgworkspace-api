using RpgWorkspace.Application.DTOs.CharacterAttribute;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterAttributeService
{
    Task<IReadOnlyList<CharacterAttributeResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterAttributeResponse> GetByIdAsync(Guid attributeId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterAttributeResponse> CreateAsync(Guid characterId, CreateCharacterAttributeRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<CharacterAttributeResponse> UpdateAsync(Guid attributeId, UpdateCharacterAttributeRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid attributeId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
