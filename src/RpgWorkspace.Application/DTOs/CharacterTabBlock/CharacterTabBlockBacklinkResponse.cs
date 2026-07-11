namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record CharacterTabBlockBacklinkResponse(
    string BlockId,
    string? BlockTitle,
    string CharacterTabId,
    string CharacterTabName
);
