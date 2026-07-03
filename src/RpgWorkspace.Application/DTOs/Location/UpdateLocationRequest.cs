using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Location;

public sealed record UpdateLocationRequest(
    [Required, MaxLength(100)] string Name,
    [Required] LocationType Type,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? Region,
    [Required] ImportanceLevel Importance,
    bool IsPrivate,
    Guid[]? TagIds = null
);
