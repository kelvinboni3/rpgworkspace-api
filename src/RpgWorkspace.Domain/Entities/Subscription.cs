using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Subscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string? Plan { get; private set; }
    public string? GatewayCustomerId { get; private set; }
    public string? GatewaySubscriptionId { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public bool ManualOverride { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    // EF Core constructor
    private Subscription() { }

    public const int DefaultTrialDays = 7;

    public static Subscription CreateNone(Guid userId)
    {
        return new Subscription
        {
            UserId = userId,
            Status = SubscriptionStatus.None,
        };
    }

    public static Subscription CreateTrial(Guid userId, DateTime trialEndsAtUtc)
    {
        return new Subscription
        {
            UserId = userId,
            Status = SubscriptionStatus.Trialing,
            CurrentPeriodEnd = trialEndsAtUtc,
        };
    }

    /// <summary>ManualOverride grants permanent free access (founder friends); it also bypasses gateway state.</summary>
    public bool IsActive() =>
        ManualOverride
        || Status is SubscriptionStatus.Active
        || (Status is SubscriptionStatus.Trialing && CurrentPeriodEnd > DateTime.UtcNow);

    public void SetManualOverride(bool enabled)
    {
        ManualOverride = enabled;
        SetUpdatedAt();
    }

    public void ApplyGatewayState(
        SubscriptionStatus status,
        string? plan,
        string? gatewayCustomerId,
        string? gatewaySubscriptionId,
        DateTime? currentPeriodEnd)
    {
        Status = status;
        Plan = plan;
        GatewayCustomerId = gatewayCustomerId;
        GatewaySubscriptionId = gatewaySubscriptionId;
        CurrentPeriodEnd = currentPeriodEnd;
        SetUpdatedAt();
    }
}
