using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Npc;

public sealed record CreateNpcRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    NpcStatus Status = NpcStatus.Unknown,
    bool IsPrivate = false,
    [MaxLength(2000)] string? Notes = null,
    Guid[]? TagIds = null
);
