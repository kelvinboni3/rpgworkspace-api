using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Session;

public sealed record UpdateSessionRequest(
    [Required, MaxLength(200)] string Title,
    [Range(1, int.MaxValue)] int Number,
    [Required] DateTime Date,
    [MaxLength(2000)] string? Summary,
    [MaxLength(2000)] string? Notes,
    [Required] SessionStatus Status,
    Guid[]? TagIds = null
);
