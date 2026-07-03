using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Npc;

public sealed record UpdateNpcRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Required] NpcStatus Status,
    bool IsPrivate,
    [MaxLength(2000)] string? Notes,
    Guid[]? TagIds = null
);
