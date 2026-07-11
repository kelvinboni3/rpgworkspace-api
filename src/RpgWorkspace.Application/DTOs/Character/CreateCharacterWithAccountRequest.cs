using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Character;

public sealed record CreateCharacterWithAccountRequest(
    [Required, MaxLength(100)] string PlayerName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required, MaxLength(100)] string CharacterName,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? Race,
    [MaxLength(100)] string? Class,
    [Range(1, 100)] int Level = 1,
    CharacterStatus Status = CharacterStatus.Active
);
