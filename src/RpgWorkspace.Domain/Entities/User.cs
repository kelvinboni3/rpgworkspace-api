using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid? DefaultCharacterId { get; private set; }

    // Navigation
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();
    private readonly List<Character> _characters = [];

    public IReadOnlyCollection<ScheduleResponse> ScheduleResponses => _scheduleResponses.AsReadOnly();
    private readonly List<ScheduleResponse> _scheduleResponses = [];

    // EF Core constructor
    private User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        return new User
        {
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
        };
    }

    public void UpdateProfile(string name)
    {
        Name = name;
        SetUpdatedAt();
    }

    public void SetDefaultCharacter(Guid? characterId)
    {
        DefaultCharacterId = characterId;
        SetUpdatedAt();
    }
}
