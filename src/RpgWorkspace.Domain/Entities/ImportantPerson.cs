using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class ImportantPerson : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ImportantPersonType Type { get; private set; }
    public string? FirstImpression { get; private set; }
    public string? Analysis { get; private set; }
    public EvaluationLevel TrustLevel { get; private set; }
    public EvaluationLevel RiskLevel { get; private set; }
    public EvaluationLevel UtilityLevel { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? LastContactAt { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;

    // EF Core constructor
    private ImportantPerson() { }

    public static ImportantPerson Create(
        Guid characterId,
        string name,
        ImportantPersonType type,
        string? firstImpression,
        string? analysis,
        EvaluationLevel trustLevel,
        EvaluationLevel riskLevel,
        EvaluationLevel utilityLevel,
        string? notes,
        DateTime? lastContactAt)
    {
        return new ImportantPerson
        {
            CharacterId = characterId,
            Name = name,
            Type = type,
            FirstImpression = firstImpression,
            Analysis = analysis,
            TrustLevel = trustLevel,
            RiskLevel = riskLevel,
            UtilityLevel = utilityLevel,
            Notes = notes,
            LastContactAt = lastContactAt,
        };
    }

    public void Update(
        string name,
        ImportantPersonType type,
        string? firstImpression,
        string? analysis,
        EvaluationLevel trustLevel,
        EvaluationLevel riskLevel,
        EvaluationLevel utilityLevel,
        string? notes,
        DateTime? lastContactAt)
    {
        Name = name;
        Type = type;
        FirstImpression = firstImpression;
        Analysis = analysis;
        TrustLevel = trustLevel;
        RiskLevel = riskLevel;
        UtilityLevel = utilityLevel;
        Notes = notes;
        LastContactAt = lastContactAt;
        SetUpdatedAt();
    }
}
