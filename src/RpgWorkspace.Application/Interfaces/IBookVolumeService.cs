using RpgWorkspace.Application.DTOs.BookVolume;

namespace RpgWorkspace.Application.Interfaces;

public interface IBookVolumeService
{
    Task<IReadOnlyList<BookVolumeResponse>> GetAllByBlockAsync(
        Guid characterTabBlockId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<BookVolumeResponse> UploadAsync(
        Guid characterTabBlockId,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        Stream content,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookVolumeResponse>> ReorderAsync(
        Guid characterTabBlockId, ReorderBookVolumesRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid volumeId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType)> GetContentAsync(
        Guid volumeId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
