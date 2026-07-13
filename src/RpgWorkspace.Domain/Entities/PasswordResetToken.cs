using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    // EF Core constructor
    private PasswordResetToken() { }

    public static PasswordResetToken Create(Guid userId, string tokenHash, DateTime expiresAt)
    {
        return new PasswordResetToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        };
    }

    public bool IsValid(DateTime nowUtc) => UsedAt is null && nowUtc <= ExpiresAt;

    public void MarkUsed()
    {
        UsedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
