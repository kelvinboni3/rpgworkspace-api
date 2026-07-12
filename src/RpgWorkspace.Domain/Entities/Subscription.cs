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

    public static Subscription CreateNone(Guid userId)
    {
        return new Subscription
        {
            UserId = userId,
            Status = SubscriptionStatus.None,
        };
    }

    /// <summary>ManualOverride is a dev-only escape hatch until a real gateway is wired up.</summary>
    public bool IsActive() => ManualOverride || Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;

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
