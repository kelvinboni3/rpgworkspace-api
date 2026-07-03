using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WorldLibraryItem;

public sealed record CreateWorldLibraryItemRequest(
    [Required, MaxLength(200)] string Title,
    WorldLibraryCategory Category = WorldLibraryCategory.Other,
    [MaxLength(2000)] string? Description = null,
    [MaxLength(5000)] string? RulesText = null,
    [MaxLength(2000)] string? Notes = null,
    WorldLibraryVisibility Visibility = WorldLibraryVisibility.Public,
    Guid[]? TagIds = null
);
