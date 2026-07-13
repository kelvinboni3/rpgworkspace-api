using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Services;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using Xunit;

namespace RpgWorkspace.Tests;

public class SubscriptionServiceTests
{
    private readonly FakeSubscriptionRepository _subscriptions = new();
    private readonly FakeSubscriptionGateway _gateway = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _service = new SubscriptionService(_subscriptions, _gateway, _unitOfWork);
    }

    // ── Character-creation gate ──────────────────────────────────────────────

    [Fact]
    public async Task Gate_allows_active_trial()
    {
        var userId = Guid.NewGuid();
        _subscriptions.Items.Add(Subscription.CreateTrial(userId, DateTime.UtcNow.AddDays(3)));

        await _service.EnsureCanCreateCharacterAsync(userId);
    }

    [Fact]
    public async Task Gate_blocks_expired_trial_with_402_exception()
    {
        var userId = Guid.NewGuid();
        _subscriptions.Items.Add(Subscription.CreateTrial(userId, DateTime.UtcNow.AddDays(-1)));

        await Assert.ThrowsAsync<SubscriptionRequiredException>(
            () => _service.EnsureCanCreateCharacterAsync(userId));
    }

    [Fact]
    public async Task Gate_allows_manual_override_founder_friends()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateNone(userId);
        subscription.SetManualOverride(true);
        _subscriptions.Items.Add(subscription);

        await _service.EnsureCanCreateCharacterAsync(userId);
    }

    [Fact]
    public async Task Gate_creates_a_row_for_legacy_users_without_one_and_blocks()
    {
        // Users created before the trial-at-signup change have no subscription row:
        // the lazy path materializes a None row, which is not active.
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<SubscriptionRequiredException>(
            () => _service.EnsureCanCreateCharacterAsync(userId));

        var created = Assert.Single(_subscriptions.Items);
        Assert.Equal(userId, created.UserId);
        Assert.Equal(SubscriptionStatus.None, created.Status);
    }

    // ── AI gate (AI is a paid perk, excluded from the trial) ─────────────────

    [Fact]
    public async Task AiGate_blocks_active_trial()
    {
        var userId = Guid.NewGuid();
        _subscriptions.Items.Add(Subscription.CreateTrial(userId, DateTime.UtcNow.AddDays(3)));

        await Assert.ThrowsAsync<SubscriptionRequiredException>(
            () => _service.EnsureAiAccessAsync(userId));
    }

    [Fact]
    public async Task AiGate_allows_paying_subscriber()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateNone(userId);
        subscription.ApplyGatewayState(SubscriptionStatus.Active, "price_x", "cus_123", "sub_123", DateTime.UtcNow.AddMonths(1));
        _subscriptions.Items.Add(subscription);

        await _service.EnsureAiAccessAsync(userId);
    }

    [Fact]
    public async Task AiGate_allows_manual_override_founder_friends()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateTrial(userId, DateTime.UtcNow.AddDays(3));
        subscription.SetManualOverride(true);
        _subscriptions.Items.Add(subscription);

        await _service.EnsureAiAccessAsync(userId);
    }

    [Fact]
    public async Task AiGate_blocks_canceled_subscription()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateNone(userId);
        subscription.ApplyGatewayState(SubscriptionStatus.Canceled, "price_x", "cus_123", "sub_123", null);
        _subscriptions.Items.Add(subscription);

        await Assert.ThrowsAsync<SubscriptionRequiredException>(
            () => _service.EnsureAiAccessAsync(userId));
    }

    // ── Webhook handling ─────────────────────────────────────────────────────

    [Fact]
    public async Task Webhook_ignores_irrelevant_event_types()
    {
        _gateway.NextWebhookEvent = null;

        await _service.HandleWebhookAsync("{}", "sig");

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Webhook_finds_subscription_by_gateway_customer_id()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateNone(userId);
        subscription.ApplyGatewayState(SubscriptionStatus.Trialing, null, "cus_123", null, null);
        _subscriptions.Items.Add(subscription);

        _gateway.NextWebhookEvent = new GatewayWebhookEvent(
            "cus_123", "sub_123", null, SubscriptionStatus.Active, "price_x", DateTime.UtcNow.AddMonths(1));

        await _service.HandleWebhookAsync("{}", "sig");

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal("sub_123", subscription.GatewaySubscriptionId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Webhook_falls_back_to_metadata_user_id_for_first_event()
    {
        // The first event for a new subscriber arrives before our row knows its
        // customer id — the userId planted in checkout metadata must resolve it.
        var userId = Guid.NewGuid();
        _subscriptions.Items.Add(Subscription.CreateTrial(userId, DateTime.UtcNow.AddDays(5)));

        _gateway.NextWebhookEvent = new GatewayWebhookEvent(
            "cus_new", "sub_new", userId, SubscriptionStatus.Active, "price_x", DateTime.UtcNow.AddMonths(1));

        await _service.HandleWebhookAsync("{}", "sig");

        var subscription = Assert.Single(_subscriptions.Items);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal("cus_new", subscription.GatewayCustomerId);
        Assert.True(subscription.IsActive());
    }

    [Fact]
    public async Task Webhook_for_unknown_customer_is_acknowledged_without_writing()
    {
        _gateway.NextWebhookEvent = new GatewayWebhookEvent(
            "cus_ghost", "sub_ghost", null, SubscriptionStatus.Active, null, null);

        await _service.HandleWebhookAsync("{}", "sig");

        Assert.Empty(_subscriptions.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Webhook_cancellation_deactivates_the_subscription()
    {
        var userId = Guid.NewGuid();
        var subscription = Subscription.CreateNone(userId);
        subscription.ApplyGatewayState(SubscriptionStatus.Active, "price_x", "cus_123", "sub_123", DateTime.UtcNow.AddMonths(1));
        _subscriptions.Items.Add(subscription);

        _gateway.NextWebhookEvent = new GatewayWebhookEvent(
            "cus_123", "sub_123", null, SubscriptionStatus.Canceled, "price_x", null);

        await _service.HandleWebhookAsync("{}", "sig");

        Assert.False(subscription.IsActive());
    }
}
