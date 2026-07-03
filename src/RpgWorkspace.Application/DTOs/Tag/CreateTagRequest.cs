using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Tag;

public sealed record CreateTagRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(30)] string? Color
);
