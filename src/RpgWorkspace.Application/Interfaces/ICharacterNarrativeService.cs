using RpgWorkspace.Application.DTOs.CharacterNarrative;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterNarrativeService
{
    Task<CharacterRecapResponse> GenerateRecapAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<CharacterRetrospectiveResponse> GenerateRetrospectiveAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>Resurfaces a random old journal block ("há X dias você escreveu isto...") — no AI call, pure DB read.</summary>
    Task<CharacterMemoryResponse?> GetMemoryAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
