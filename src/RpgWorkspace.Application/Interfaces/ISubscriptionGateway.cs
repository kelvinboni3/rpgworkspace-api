using RpgWorkspace.Application.DTOs.Subscription;

namespace RpgWorkspace.Application.Interfaces;

/// <summary>
/// Abstraction over the payment gateway (Stripe). Without credentials configured, the
/// implementation throws NotSupportedException, which the controller maps to 501.
/// </summary>
public interface ISubscriptionGateway
{
    Task<string> CreateCheckoutSessionAsync(Guid userId, string plan, CancellationToken cancellationToken = default);

    /// <summary>Returns null for event types that don't affect subscription state (acknowledged and ignored).</summary>
    Task<GatewayWebhookEvent?> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken = default);
}
