using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record CreateCharacterTabBlockRequest(
    [Required] CharacterTabBlockType Type,
    [MaxLength(200)] string? Title,
    [MaxLength(200)] string? Meta,
    [MaxLength(10000)] string? Content,
    string? PayloadJson,
    [MaxLength(20)] string? AccentColor = null
);
