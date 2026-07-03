using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Session;

public sealed record SessionResponse(
    string Id,
    string CampaignId,
    string Title,
    int Number,
    DateTime Date,
    string? Summary,
    string? Notes,
    SessionStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
