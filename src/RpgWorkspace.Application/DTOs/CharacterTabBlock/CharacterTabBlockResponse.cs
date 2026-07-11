using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record CharacterTabBlockResponse(
    string Id,
    string CharacterTabId,
    string? ParentBlockId,
    CharacterTabBlockType Type,
    int Order,
    string? Title,
    string? Meta,
    string? Content,
    string? PayloadJson,
    string? AccentColor,
    string? ImageUrl,
    IReadOnlyList<CharacterTabBlockResponse> Children,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
