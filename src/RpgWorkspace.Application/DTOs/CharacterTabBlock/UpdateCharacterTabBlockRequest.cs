using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record UpdateCharacterTabBlockRequest(
    [MaxLength(200)] string? Title,
    [MaxLength(200)] string? Meta,
    [MaxLength(50000)] string? Content,
    string? PayloadJson
);
