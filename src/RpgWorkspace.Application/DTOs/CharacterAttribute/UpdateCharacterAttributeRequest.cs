using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterAttribute;

public sealed record UpdateCharacterAttributeRequest(
    [Required, MaxLength(50)] string Name,
    [Required, MaxLength(100)] string Value
);
