namespace RpgWorkspace.Application.DTOs.Character;

public sealed record CharacterWithAccountResponse(
    CharacterResponse Character,
    string UserId,
    string PlayerName,
    string Email
);
