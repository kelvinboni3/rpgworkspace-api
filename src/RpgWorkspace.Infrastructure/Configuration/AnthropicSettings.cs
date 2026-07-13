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

    /// <summary>Recap is 1-2 paragraphs, but a rich journal legitimately needs ~700 tokens of JSON;
    /// 512 truncated mid-JSON on real data (structured outputs can't survive max_tokens cuts).</summary>
    public int RecapMaxOutputTokens { get; init; } = 1536;

    /// <summary>Retrospective is 1-3 paragraphs — longer than a recap, same truncation risk.</summary>
    public int RetrospectiveMaxOutputTokens { get; init; } = 2048;

    public int RateLimitPerHour { get; init; } = 20;
}
