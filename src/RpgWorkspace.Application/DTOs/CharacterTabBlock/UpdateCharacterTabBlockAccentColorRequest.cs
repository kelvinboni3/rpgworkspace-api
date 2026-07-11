using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record UpdateCharacterTabBlockAccentColorRequest(
    [MaxLength(20)] string? AccentColor
);
