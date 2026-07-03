using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Character;

public sealed record CharacterResponse(
    string Id,
    string CampaignId,
    string UserId,
    string Name,
    string? Description,
    string? Race,
    string? Class,
    int Level,
    CharacterStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
