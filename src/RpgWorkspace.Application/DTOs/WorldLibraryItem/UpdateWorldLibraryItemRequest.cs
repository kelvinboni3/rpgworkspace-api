using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorldLibraryItem;

public sealed record UpdateWorldLibraryItemRequest(
    [Required, MaxLength(200)] string Title,
    [Required] WorldLibraryCategory Category,
    [MaxLength(2000)] string? Description,
    [MaxLength(5000)] string? RulesText,
    [MaxLength(2000)] string? Notes,
    [Required] WorldLibraryVisibility Visibility,
    Guid[]? TagIds = null
);
