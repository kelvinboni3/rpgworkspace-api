using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Character : BaseEntity
{
    public Guid? CampaignId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Race { get; private set; }
    public string? Class { get; private set; }
    public int Level { get; private set; }
    public CharacterStatus Status { get; private set; }
    public Guid? PortraitAssetId { get; private set; }
    public int? HpCurrent { get; private set; }
    public int? HpMax { get; private set; }
    public int? MpCurrent { get; private set; }
    public int? MpMax { get; private set; }

    public bool IsSolo => CampaignId is null;

    // Navigation
    public Campaign? Campaign { get; private set; }
    public User User { get; private set; } = null!;

    // EF Core constructor
    private Character() { }

    public static Character Create(
        Guid? campaignId,
        Guid userId,
        string name,
        string? description,
        string? race,
        string? characterClass,
        int level,
        CharacterStatus status)
    {
        return new Character
        {
            CampaignId = campaignId,
            UserId = userId,
            Name = name,
            Description = description,
            Race = race,
            Class = characterClass,
            Level = level,
            Status = status,
        };
    }

    public void Update(
        string name,
        string? description,
        string? race,
        string? characterClass,
        int level,
        CharacterStatus status)
    {
        Name = name;
        Description = description;
        Race = race;
        Class = characterClass;
        Level = level;
        Status = status;
        SetUpdatedAt();
    }

    public void SetPortrait(Guid? portraitAssetId)
    {
        PortraitAssetId = portraitAssetId;
        SetUpdatedAt();
    }

    public void UpdateVitals(int? hpCurrent, int? hpMax, int? mpCurrent, int? mpMax)
    {
        HpCurrent = hpCurrent;
        HpMax = hpMax;
        MpCurrent = mpCurrent;
        MpMax = mpMax;
        SetUpdatedAt();
    }
}
