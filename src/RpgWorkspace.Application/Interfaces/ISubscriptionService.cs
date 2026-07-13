using RpgWorkspace.Application.DTOs.Subscription;

namespace RpgWorkspace.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CheckoutSessionResponse> StartCheckoutAsync(Guid userId, StartCheckoutRequest request, CancellationToken cancellationToken = default);
    Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    Task<SubscriptionResponse> SetManualOverrideAsync(Guid userId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>Throws SubscriptionRequiredException if the user has no active subscription and has already used the free solo character.</summary>
    Task EnsureCanCreateCharacterAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Throws SubscriptionRequiredException unless the user is a paying subscriber (or ManualOverride) — AI is excluded from the trial.</summary>
    Task EnsureAiAccessAsync(Guid userId, CancellationToken cancellationToken = default);
}
