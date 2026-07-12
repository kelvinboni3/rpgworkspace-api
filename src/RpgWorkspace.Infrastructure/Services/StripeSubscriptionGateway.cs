using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Infrastructure.Services;

/// <summary>
/// Stub gateway: no Stripe (or other) account/credentials exist yet. Every call fails loudly
/// instead of pretending to work, so callers (SubscriptionsController) can map it to a clear
/// 501 rather than a confusing 500.
/// </summary>
public sealed class StripeSubscriptionGateway : ISubscriptionGateway
{
    public Task<string> CreateCheckoutSessionAsync(Guid userId, string plan, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Payment gateway is not configured yet.");

    public Task<GatewayWebhookEvent> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Payment gateway is not configured yet.");
}
