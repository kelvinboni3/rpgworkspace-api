using RpgWorkspace.Domain.Common;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Domain.Entities;

public sealed class Operation : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Objective { get; private set; }
    public string? Plan { get; private set; }
    public string? RequiredResources { get; private set; }
    public string? Risks { get; private set; }
    public OperationStatus Status { get; private set; }
    public string? Result { get; private set; }

    // Navigation
    public Character Character { get; private set; } = null!;

    // EF Core constructor
    private Operation() { }

    public static Operation Create(
        Guid characterId,
        string name,
        string? objective,
        string? plan,
        string? requiredResources,
        string? risks,
        OperationStatus status,
        string? result)
    {
        return new Operation
        {
            CharacterId = characterId,
            Name = name,
            Objective = objective,
            Plan = plan,
            RequiredResources = requiredResources,
            Risks = risks,
            Status = status,
            Result = result,
        };
    }

    public void Update(
        string name,
        string? objective,
        string? plan,
        string? requiredResources,
        string? risks,
        OperationStatus status,
        string? result)
    {
        Name = name;
        Objective = objective;
        Plan = plan;
        RequiredResources = requiredResources;
        Risks = risks;
        Status = status;
        Result = result;
        SetUpdatedAt();
    }
}
