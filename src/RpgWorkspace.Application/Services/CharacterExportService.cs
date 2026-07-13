using System.Text;
using System.Text.Json;
using RpgWorkspace.Application.DTOs.CharacterExport;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterExportService : ICharacterExportService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterTabBlockRepository _characterTabBlockRepository;

    public CharacterExportService(
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterTabBlockRepository characterTabBlockRepository)
    {
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _characterTabRepository = characterTabRepository;
        _characterTabBlockRepository = characterTabBlockRepository;
    }

    public async Task<MarkdownExportResult> ExportMarkdownAsync(
        Guid characterId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken)
            ?? throw new KeyNotFoundException("Character not found.");

        var workspace = await CharacterAuthorizationHelper.ResolveWorkspaceAsync(
            _campaignRepository, _workspaceRepository, character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Character not found.");

        var tabs = await _characterTabRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine($"# {character.Name}");

        var subtitle = string.Join(
            " · ",
            new[] { $"Nível {character.Level}", character.Race, character.Class }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        if (subtitle.Length > 0)
            sb.AppendLine(subtitle);

        if (!string.IsNullOrWhiteSpace(character.Description))
        {
            sb.AppendLine();
            sb.AppendLine($"> {character.Description}");
        }

        sb.AppendLine();

        foreach (var tab in tabs.OrderBy(t => t.Order))
        {
            var blocks = await _characterTabBlockRepository.GetAllByTabAsync(tab.Id, cancellationToken);
            if (blocks.Count == 0)
                continue;

            sb.AppendLine($"## {tab.Name}");
            sb.AppendLine();

            foreach (var block in blocks.OrderBy(b => b.Order))
                AppendBlock(sb, block, 3);

            sb.AppendLine();
        }

        return new MarkdownExportResult($"{Slugify(character.Name)}.md", sb.ToString());
    }

    private static void AppendBlock(StringBuilder sb, CharacterTabBlock block, int headingLevel)
    {
        var heading = new string('#', Math.Min(headingLevel, 6));

        switch (block.Type)
        {
            case CharacterTabBlockType.Divider:
                sb.AppendLine("---");
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Quote:
                if (!string.IsNullOrWhiteSpace(block.Title))
                    sb.AppendLine($"{heading} {block.Title}");
                foreach (var line in (block.Content ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
                    sb.AppendLine($"> {line}");
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Card:
                sb.AppendLine($"{heading} {block.Title ?? "Card"}");
                foreach (var (key, value) in ParseCardRows(block.PayloadJson))
                    sb.AppendLine($"- **{key}**: {value}");
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Table:
                sb.AppendLine($"{heading} {block.Title ?? "Tabela"}");
                AppendTable(sb, block.PayloadJson);
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Image:
                var imageCaption = ParseStringProperty(block.PayloadJson, "caption");
                sb.AppendLine(string.IsNullOrWhiteSpace(imageCaption)
                    ? "*(Imagem — não incluída na exportação)*"
                    : $"*(Imagem: {imageCaption} — não incluída na exportação)*");
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Book:
                var coverCaption = ParseStringProperty(block.PayloadJson, "coverCaption");
                sb.AppendLine(string.IsNullOrWhiteSpace(coverCaption)
                    ? "*(Livro — não incluído na exportação)*"
                    : $"*(Livro: {coverCaption} — não incluído na exportação)*");
                sb.AppendLine();
                break;

            case CharacterTabBlockType.Text:
            case CharacterTabBlockType.Collapse:
            default:
                if (!string.IsNullOrWhiteSpace(block.Title))
                    sb.AppendLine($"{heading} {block.Title}");
                if (!string.IsNullOrWhiteSpace(block.Content))
                {
                    sb.AppendLine(block.Content);
                    sb.AppendLine();
                }
                break;
        }

        foreach (var child in block.Children.OrderBy(c => c.Order))
            AppendBlock(sb, child, headingLevel + 1);
    }

    private static IEnumerable<(string Key, string Value)> ParseCardRows(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            yield break;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payloadJson);
        }
        catch (JsonException)
        {
            yield break;
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
                yield break;

            foreach (var row in rows.EnumerateArray())
            {
                var key = row.TryGetProperty("k", out var k) ? k.GetString() : null;
                var value = row.TryGetProperty("v", out var v) ? v.GetString() : null;
                if (!string.IsNullOrWhiteSpace(key))
                    yield return (key!, value ?? string.Empty);
            }
        }
    }

    private static void AppendTable(StringBuilder sb, string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payloadJson);
        }
        catch (JsonException)
        {
            return;
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("headers", out var headersEl) || headersEl.ValueKind != JsonValueKind.Array)
                return;

            var headers = headersEl.EnumerateArray().Select(h => h.GetString() ?? string.Empty).ToList();
            if (headers.Count == 0)
                return;

            sb.AppendLine($"| {string.Join(" | ", headers)} |");
            sb.AppendLine($"| {string.Join(" | ", headers.Select(_ => "---"))} |");

            if (doc.RootElement.TryGetProperty("rows", out var rowsEl) && rowsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in rowsEl.EnumerateArray())
                {
                    if (row.ValueKind != JsonValueKind.Array)
                        continue;

                    var cells = row.EnumerateArray().Select(c => (c.GetString() ?? string.Empty).Replace("|", "\\|"));
                    sb.AppendLine($"| {string.Join(" | ", cells)} |");
                }
            }
        }
    }

    private static string? ParseStringProperty(string? payloadJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            return doc.RootElement.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Slugify(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var chars = normalized.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        slug = slug.Trim('-');
        return string.IsNullOrEmpty(slug) ? "personagem" : slug;
    }
}
