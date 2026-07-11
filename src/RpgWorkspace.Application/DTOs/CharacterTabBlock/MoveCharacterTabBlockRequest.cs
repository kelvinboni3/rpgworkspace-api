using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record MoveCharacterTabBlockRequest(
    [Required] string Direction
);
