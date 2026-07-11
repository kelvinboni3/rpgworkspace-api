namespace RpgWorkspace.Infrastructure.Configuration;

public sealed class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-haiku-4-5";
    public int MaxOutputTokens { get; init; } = 1024;
    public int RateLimitPerHour { get; init; } = 20;
}
