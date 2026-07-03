using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Npc;

public sealed record NpcResponse(
    string Id,
    string CampaignId,
    string Name,
    string? Description,
    NpcStatus Status,
    bool IsPrivate,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
