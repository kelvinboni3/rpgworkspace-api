using Microsoft.Extensions.Options;
using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Infrastructure.Configuration;
using Stripe;
using Stripe.Checkout;

namespace RpgWorkspace.Infrastructure.Services;

/// <summary>
/// Real Stripe gateway. Without a configured SecretKey it throws NotSupportedException on every
/// call, which SubscriptionsController maps to 501 — so a deploy without keys stays harmless.
/// </summary>
public sealed class StripeSubscriptionGateway : ISubscriptionGateway
{
    private readonly StripeSettings _settings;

    public StripeSubscriptionGateway(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid userId, string plan, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var client = new StripeClient(_settings.SecretKey);
        var sessions = new SessionService(client);

        try
        {
            return await CreateSessionAsync(sessions, userId, cancellationToken);
        }
        catch (StripeException ex)
        {
            // Account under review, payment methods disabled, Stripe outage — the frontend
            // already renders 501 as "payments not available yet", which is the honest state.
            throw new NotSupportedException("Payment gateway is temporarily unavailable.", ex);
        }
    }

    private async Task<string> CreateSessionAsync(SessionService sessions, Guid userId, CancellationToken cancellationToken)
    {
        var session = await sessions.CreateAsync(new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions { Price = _settings.PriceId, Quantity = 1 },
            ],
            // Both ids let the webhook find our Subscription row before it knows its customer id.
            ClientReferenceId = userId.ToString(),
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string> { ["userId"] = userId.ToString() },
            },
            SuccessUrl = $"{_settings.FrontendBaseUrl}/subscription?checkout=success",
            CancelUrl = $"{_settings.FrontendBaseUrl}/subscription?checkout=canceled",
        }, cancellationToken: cancellationToken);

        return session.Url;
    }

    public Task<GatewayWebhookEvent?> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _settings.WebhookSecret);
        }
        catch (StripeException ex)
        {
            throw new InvalidWebhookSignatureException("Invalid Stripe webhook signature.", ex);
        }

        // Only customer.subscription.* events carry the state we persist; everything else is acknowledged.
        if (stripeEvent.Data.Object is not Stripe.Subscription subscription)
            return Task.FromResult<GatewayWebhookEvent?>(null);

        var status = subscription.Status switch
        {
            "trialing" => SubscriptionStatus.Trialing,
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" or "unpaid" or "incomplete_expired" => SubscriptionStatus.Canceled,
            _ => SubscriptionStatus.None,
        };

        Guid? userId = subscription.Metadata is not null
            && subscription.Metadata.TryGetValue("userId", out var rawUserId)
            && Guid.TryParse(rawUserId, out var parsed)
                ? parsed
                : null;

        var firstItem = subscription.Items?.Data?.FirstOrDefault();

        return Task.FromResult<GatewayWebhookEvent?>(new GatewayWebhookEvent(
            subscription.CustomerId,
            subscription.Id,
            userId,
            status,
            firstItem?.Price?.Id,
            firstItem?.CurrentPeriodEnd));
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new NotSupportedException("Payment gateway is not configured yet.");
    }
}
