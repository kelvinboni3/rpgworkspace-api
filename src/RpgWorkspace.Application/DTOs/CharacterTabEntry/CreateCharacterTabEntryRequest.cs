using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTabEntry;

public sealed record CreateCharacterTabEntryRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(5000)] string Content
);
