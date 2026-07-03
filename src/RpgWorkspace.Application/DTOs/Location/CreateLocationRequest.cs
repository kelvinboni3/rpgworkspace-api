using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Location;

public sealed record CreateLocationRequest(
    [Required, MaxLength(100)] string Name,
    [Required] LocationType Type,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? Region,
    ImportanceLevel Importance = ImportanceLevel.Medium,
    bool IsPrivate = false,
    Guid[]? TagIds = null
);
