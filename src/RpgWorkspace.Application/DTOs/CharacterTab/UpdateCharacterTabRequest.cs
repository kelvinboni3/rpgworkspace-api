using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTab;

public sealed record UpdateCharacterTabRequest(
    [Required, MaxLength(100)] string Name
);
