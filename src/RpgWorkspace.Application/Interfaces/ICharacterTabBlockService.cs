using RpgWorkspace.Application.DTOs.CharacterTabBlock;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterTabBlockService
{
    Task<IReadOnlyList<CharacterTabBlockResponse>> GetAllByTabAsync(
        Guid characterTabId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> GetByIdAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> CreateAsync(
        Guid characterTabId, CreateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> CreateChildAsync(
        Guid parentBlockId, CreateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> UpdateAsync(
        Guid blockId, UpdateCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> UpdateAccentColorAsync(
        Guid blockId, UpdateCharacterTabBlockAccentColorRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CharacterTabBlockResponse>> MoveAsync(
        Guid blockId, MoveCharacterTabBlockRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CharacterTabBlockResponse>> ReorderAsync(
        Guid characterTabId, ReorderCharacterTabBlocksRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CharacterTabBlockResponse>> ReorderChildrenAsync(
        Guid parentBlockId, ReorderCharacterTabBlocksRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> SetParentAsync(
        Guid blockId, SetCharacterTabBlockParentRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> SetImageAsync(
        Guid blockId, string originalFileName, string contentType, long fileSizeBytes, Stream content,
        Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<CharacterTabBlockResponse> RemoveImageAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType)> GetImageContentAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CharacterTabBlockBacklinkResponse>> GetBacklinksAsync(
        Guid blockId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
