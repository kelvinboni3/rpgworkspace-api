using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WikiPage;

public sealed record UpdateWikiPageRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content,
    [Required] WikiVisibility Visibility,
    Guid[]? TagIds = null
);
