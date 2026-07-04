namespace RpgWorkspace.Application.DTOs.Character;

public sealed record UpdateCharacterVitalsRequest(
    int? HpCurrent,
    int? HpMax,
    int? MpCurrent,
    int? MpMax
);
