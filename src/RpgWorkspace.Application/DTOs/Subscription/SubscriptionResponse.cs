using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Subscription;

public sealed record SubscriptionResponse(
    string UserId,
    SubscriptionStatus Status,
    string? Plan,
    DateTime? CurrentPeriodEnd,
    bool ManualOverride,
    bool IsActive,
    bool HasAiAccess
);
