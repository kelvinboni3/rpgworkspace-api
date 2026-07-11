using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class BookVolume : BaseEntity
{
    public Guid CharacterTabBlockId { get; private set; }
    public int Order { get; private set; }
    public Guid MediaAssetId { get; private set; }

    // Navigation
    public MediaAsset MediaAsset { get; private set; } = null!;

    // EF Core constructor
    private BookVolume() { }

    public static BookVolume Create(Guid characterTabBlockId, int order, Guid mediaAssetId)
    {
        return new BookVolume
        {
            CharacterTabBlockId = characterTabBlockId,
            Order = order,
            MediaAssetId = mediaAssetId,
        };
    }

    public void Reorder(int order)
    {
        Order = order;
        SetUpdatedAt();
    }
}
