using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.NarrativeItem;

public sealed record CreateNarrativeItemRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [MaxLength(500)] string? Origin,
    Guid? SessionId,
    NarrativeItemImportance Importance = NarrativeItemImportance.Medium,
    [MaxLength(2000)] string? Notes = null,
    Guid[]? TagIds = null
);
