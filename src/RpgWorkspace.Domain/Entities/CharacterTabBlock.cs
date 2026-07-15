using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterTabBlock : BaseEntity
{
    public Guid CharacterTabId { get; private set; }
    public CharacterTabBlockType Type { get; private set; }
    public int Order { get; private set; }
    public string? Title { get; private set; }
    public string? Meta { get; private set; }
    public string? Content { get; private set; }
    public string? PayloadJson { get; private set; }
    public Guid? ParentBlockId { get; private set; }
    public string? AccentColor { get; private set; }
    public Guid? ImageAssetId { get; private set; }

    // Navigation
    public CharacterTab CharacterTab { get; private set; } = null!;

    public IReadOnlyCollection<CharacterTabBlock> Children => _children.AsReadOnly();
    private readonly List<CharacterTabBlock> _children = [];

    // EF Core constructor
    private CharacterTabBlock() { }

    public static CharacterTabBlock Create(
        Guid characterTabId,
        CharacterTabBlockType type,
        int order,
        string? title,
        string? meta,
        string? content,
        string? payloadJson,
        Guid? parentBlockId = null,
        string? accentColor = null)
    {
        return new CharacterTabBlock
        {
            CharacterTabId = characterTabId,
            Type = type,
            Order = order,
            Title = title,
            Meta = meta,
            Content = content,
            PayloadJson = payloadJson,
            ParentBlockId = parentBlockId,
            AccentColor = accentColor,
        };
    }

    public void Update(string? title, string? meta, string? content, string? payloadJson)
    {
        Title = title;
        Meta = meta;
        Content = content;
        PayloadJson = payloadJson;
        SetUpdatedAt();
    }

    public void SetAccentColor(string? accentColor)
    {
        AccentColor = accentColor;
        SetUpdatedAt();
    }

    public void Reorder(int order)
    {
        Order = order;
        SetUpdatedAt();
    }

    public void SetParent(Guid? parentBlockId, int order)
    {
        ParentBlockId = parentBlockId;
        Order = order;
        SetUpdatedAt();
    }

    public void SetImage(Guid? imageAssetId)
    {
        ImageAssetId = imageAssetId;
        SetUpdatedAt();
    }
}
