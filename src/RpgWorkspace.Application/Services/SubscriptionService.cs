using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionGateway _subscriptionGateway;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionGateway subscriptionGateway,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _subscriptionGateway = subscriptionGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetOrCreateAsync(userId, cancellationToken);
        return ToResponse(subscription);
    }

    public async Task<CheckoutSessionResponse> StartCheckoutAsync(
        Guid userId, StartCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var checkoutUrl = await _subscriptionGateway.CreateCheckoutSessionAsync(userId, request.Plan, cancellationToken);
        return new CheckoutSessionResponse(checkoutUrl);
    }

    public async Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        var webhookEvent = await _subscriptionGateway.ParseWebhookEventAsync(payload, signature, cancellationToken);
        if (webhookEvent is null)
            return; // Event type we don't care about — acknowledge so the gateway stops retrying.

        // The first event for a new subscriber arrives before our row knows its gateway customer id,
        // so fall back to the userId we planted in the subscription's metadata at checkout.
        var subscription = webhookEvent.GatewayCustomerId is not null
            ? await _subscriptionRepository.GetByGatewayCustomerIdAsync(webhookEvent.GatewayCustomerId, cancellationToken)
            : null;

        if (subscription is null && webhookEvent.UserId is Guid userId)
            subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (subscription is null)
            return; // Unknown customer (e.g. deleted account) — nothing to update.

        subscription.ApplyGatewayState(
            webhookEvent.Status, webhookEvent.Plan, webhookEvent.GatewayCustomerId,
            webhookEvent.GatewaySubscriptionId, webhookEvent.CurrentPeriodEnd);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubscriptionResponse> SetManualOverrideAsync(
        Guid userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var subscription = await GetOrCreateAsync(userId, cancellationToken);
        subscription.SetManualOverride(enabled);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(subscription);
    }

    public async Task EnsureCanCreateCharacterAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetOrCreateAsync(userId, cancellationToken);
        if (subscription.IsActive())
            return;

        throw new SubscriptionRequiredException(
            "Your trial has ended. Subscribe to keep creating characters.");
    }

    public async Task EnsureAiAccessAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetOrCreateAsync(userId, cancellationToken);
        if (subscription.HasPaidAccess())
            return;

        throw new SubscriptionRequiredException(
            "AI features are not included in the trial. Subscribe to unlock them.");
    }

    private async Task<Subscription> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscription is not null)
            return subscription;

        subscription = Subscription.CreateNone(userId);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return subscription;
    }

    private static SubscriptionResponse ToResponse(Subscription s) =>
        new(s.UserId.ToString(), s.Status, s.Plan, s.CurrentPeriodEnd, s.ManualOverride, s.IsActive(), s.HasPaidAccess());
}
