namespace RpgWorkspace.Infrastructure.Configuration;

public sealed class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-haiku-4-5";
    public int MaxOutputTokens { get; init; } = 1024;

    /// <summary>Higher cap for sheet import: extracting a full character sheet produces many more
    /// blocks in one call than a single post-session note does.</summary>
    public int ImportMaxOutputTokens { get; init; } = 4096;

    public int RateLimitPerHour { get; init; } = 20;
}
