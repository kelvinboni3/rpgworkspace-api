namespace RpgWorkspace.Application.DTOs.CharacterNarrative;

public sealed record CharacterMemoryResponse(
    string BlockId,
    string TabId,
    string TabName,
    string? Title,
    string Content,
    DateTime CreatedAt
);
