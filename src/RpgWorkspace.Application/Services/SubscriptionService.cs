using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private const int FreeSoloCharacterLimit = 1;

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ISubscriptionGateway _subscriptionGateway;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        ICharacterRepository characterRepository,
        ISubscriptionGateway subscriptionGateway,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _characterRepository = characterRepository;
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

        var subscription = await _subscriptionRepository.GetByGatewayCustomerIdAsync(webhookEvent.GatewayCustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found for gateway customer.");

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

        var soloCount = await _characterRepository.CountSoloByUserAsync(userId, cancellationToken);
        if (soloCount < FreeSoloCharacterLimit)
            return;

        throw new SubscriptionRequiredException(
            "Free plan allows 1 solo character. Subscribe to create more.");
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
        new(s.UserId.ToString(), s.Status, s.Plan, s.CurrentPeriodEnd, s.ManualOverride, s.IsActive());
}
