using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Theory;

public sealed record UpdateTheoryRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    [MaxLength(3000)] string? Evidence,
    [Range(0, 100)] int Confidence,
    [Required] TheoryStatus Status,
    Guid[]? TagIds = null
);
