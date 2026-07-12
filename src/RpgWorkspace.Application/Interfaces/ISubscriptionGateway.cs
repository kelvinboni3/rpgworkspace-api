using RpgWorkspace.Application.DTOs.Subscription;

namespace RpgWorkspace.Application.Interfaces;

/// <summary>
/// Abstraction over the payment gateway (Stripe or otherwise). No implementation is wired to
/// real credentials yet — see StripeSubscriptionGateway, which throws NotSupportedException
/// until a real account exists.
/// </summary>
public interface ISubscriptionGateway
{
    Task<string> CreateCheckoutSessionAsync(Guid userId, string plan, CancellationToken cancellationToken = default);
    Task<GatewayWebhookEvent> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken = default);
}
