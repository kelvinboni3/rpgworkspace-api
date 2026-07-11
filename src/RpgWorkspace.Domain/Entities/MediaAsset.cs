using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class MediaAsset : BaseEntity
{
    public byte[] Content { get; private set; } = [];
    public string ContentType { get; private set; } = string.Empty;
    public string? OriginalFileName { get; private set; }
    public long FileSizeBytes { get; private set; }

    // EF Core constructor
    private MediaAsset() { }

    public static MediaAsset Create(byte[] content, string contentType, string? originalFileName, long fileSizeBytes)
    {
        return new MediaAsset
        {
            Content = content,
            ContentType = contentType,
            OriginalFileName = originalFileName,
            FileSizeBytes = fileSizeBytes,
        };
    }
}
