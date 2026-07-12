using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Character;

public sealed record CreateSoloCharacterRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? Race,
    [MaxLength(100)] string? Class,
    [Range(1, 100)] int Level = 1,
    CharacterStatus Status = CharacterStatus.Active
);
