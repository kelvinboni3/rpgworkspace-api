namespace RpgWorkspace.Infrastructure.Configuration;

public sealed class ResendSettings
{
    public const string SectionName = "Resend";

    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "onboarding@resend.dev";
    public string FromName { get; init; } = "Aventurário";
    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";
    public int RateLimitPerHour { get; init; } = 5;
}
