namespace RpgWorkspace.Infrastructure.Configuration;

public sealed class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string PriceId { get; init; } = string.Empty;
    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";
}
