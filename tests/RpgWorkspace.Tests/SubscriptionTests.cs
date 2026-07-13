using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;
using Xunit;

namespace RpgWorkspace.Tests;

public class SubscriptionTests
{
    [Fact]
    public void CreateNone_is_not_active()
    {
        var subscription = Subscription.CreateNone(Guid.NewGuid());

        Assert.Equal(SubscriptionStatus.None, subscription.Status);
        Assert.False(subscription.IsActive());
    }

    [Fact]
    public void CreateTrial_is_active_until_it_expires()
    {
        var subscription = Subscription.CreateTrial(Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        Assert.Equal(SubscriptionStatus.Trialing, subscription.Status);
        Assert.True(subscription.IsActive());
    }

    [Fact]
    public void Expired_trial_is_not_active()
    {
        // The original bug: a stale Trialing subscription read as active forever.
        var subscription = Subscription.CreateTrial(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1));

        Assert.Equal(SubscriptionStatus.Trialing, subscription.Status);
        Assert.False(subscription.IsActive());
    }

    [Fact]
    public void Gateway_active_state_is_active_and_canceled_is_not()
    {
        var subscription = Subscription.CreateNone(Guid.NewGuid());

        subscription.ApplyGatewayState(SubscriptionStatus.Active, "price_x", "cus_x", "sub_x", DateTime.UtcNow.AddMonths(1));
        Assert.True(subscription.IsActive());

        subscription.ApplyGatewayState(SubscriptionStatus.Canceled, "price_x", "cus_x", "sub_x", null);
        Assert.False(subscription.IsActive());
    }

    [Fact]
    public void ManualOverride_grants_access_even_when_expired_or_canceled()
    {
        var expiredTrial = Subscription.CreateTrial(Guid.NewGuid(), DateTime.UtcNow.AddDays(-30));
        expiredTrial.SetManualOverride(true);
        Assert.True(expiredTrial.IsActive());

        var canceled = Subscription.CreateNone(Guid.NewGuid());
        canceled.ApplyGatewayState(SubscriptionStatus.Canceled, null, "cus_y", "sub_y", null);
        canceled.SetManualOverride(true);
        Assert.True(canceled.IsActive());
    }
}
