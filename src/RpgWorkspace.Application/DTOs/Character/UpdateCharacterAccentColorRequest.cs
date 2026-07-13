using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Character;

public sealed record UpdateCharacterAccentColorRequest(
    [MaxLength(20)] string? AccentColor
);
