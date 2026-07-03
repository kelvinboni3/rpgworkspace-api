namespace RpgWorkspace.Application.DTOs.Search;

public sealed record SearchResultResponse(
    string Id,
    string Type,
    string Title,
    string? Description,
    string Url,
    bool IsPrivate
);
