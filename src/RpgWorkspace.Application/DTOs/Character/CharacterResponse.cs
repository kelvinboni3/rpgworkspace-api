using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Character;

public sealed record CharacterResponse(
    string Id,
    string? CampaignId,
    string UserId,
    string Name,
    string? Description,
    string? Race,
    string? Class,
    int Level,
    CharacterStatus Status,
    string? PortraitUrl,
    int? HpCurrent,
    int? HpMax,
    int? MpCurrent,
    int? MpMax,
    string? RetrospectiveText,
    string? RecapText,
    DateTime? RecapGeneratedAt,
    string? AccentColor,
    string? PublicShareToken,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
