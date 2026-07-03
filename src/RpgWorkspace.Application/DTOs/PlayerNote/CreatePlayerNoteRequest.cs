using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.PlayerNote;

public sealed record CreatePlayerNoteRequest(
    Guid? SessionId,
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(5000)] string Content,
    [MaxLength(500)] string? Tags,
    Guid[]? TagIds = null
);
