using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Subscription;

public sealed record GatewayWebhookEvent(
    string? GatewayCustomerId,
    string? GatewaySubscriptionId,
    Guid? UserId,
    SubscriptionStatus Status,
    string? Plan,
    DateTime? CurrentPeriodEnd
);
