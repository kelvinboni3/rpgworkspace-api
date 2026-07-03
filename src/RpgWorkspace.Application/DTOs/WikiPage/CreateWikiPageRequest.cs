using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.WikiPage;

public sealed record CreateWikiPageRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content,
    WikiVisibility Visibility = WikiVisibility.Public,
    Guid[]? TagIds = null
);
