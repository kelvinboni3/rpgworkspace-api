using RpgWorkspace.Application.DTOs.Public;

namespace RpgWorkspace.Application.Interfaces;

public interface IPublicCharacterService
{
    Task<PublicCharacterResponse> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType)> GetPortraitAsync(string token, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType)> GetBlockImageAsync(string token, Guid blockId, CancellationToken cancellationToken = default);
}
