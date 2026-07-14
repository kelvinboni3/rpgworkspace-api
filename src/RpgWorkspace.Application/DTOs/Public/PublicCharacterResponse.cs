using RpgWorkspace.Application.DTOs.CharacterTabBlock;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Public;

public sealed record PublicCharacterResponse(
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
    string? AccentColor,
    IReadOnlyList<PublicCharacterTabResponse> Tabs
);

public sealed record PublicCharacterTabResponse(
    string Name,
    IReadOnlyList<CharacterTabBlockResponse> Blocks
);
