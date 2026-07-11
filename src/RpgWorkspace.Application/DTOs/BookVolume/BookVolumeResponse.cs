namespace RpgWorkspace.Application.DTOs.BookVolume;

public sealed record BookVolumeResponse(
    string Id,
    string CharacterTabBlockId,
    int Order,
    string OriginalFileName,
    string FileUrl,
    long FileSizeBytes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
