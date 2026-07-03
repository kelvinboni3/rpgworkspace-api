using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.NarrativeItem;

public sealed record UpdateNarrativeItemRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [MaxLength(500)] string? Origin,
    Guid? SessionId,
    [Required] NarrativeItemImportance Importance,
    [MaxLength(2000)] string? Notes,
    Guid[]? TagIds = null
);
