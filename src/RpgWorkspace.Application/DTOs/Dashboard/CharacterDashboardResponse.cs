using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Dashboard;

public sealed record CharacterDashboardResponse(
    string CharacterId,
    string CharacterName,
    string CampaignId,
    string CampaignName,
    CharacterDashboardLastPlayerNoteResponse? LastPlayerNote,
    int ActiveTheoriesCount,
    int ActiveOperationsCount,
    int ImportantPeopleCount,
    int NarrativeItemsCount,
    IReadOnlyList<CharacterDashboardPlayerNoteResponse> RecentNotes,
    IReadOnlyList<CharacterDashboardTheoryResponse> ActiveTheories,
    IReadOnlyList<CharacterDashboardOperationResponse> ActiveOperations,
    IReadOnlyList<CharacterDashboardImportantPersonResponse> ImportantPeopleHighlights
);

public sealed record CharacterDashboardLastPlayerNoteResponse(
    string Id,
    string Title,
    DateTime CreatedAt
);

public sealed record CharacterDashboardPlayerNoteResponse(
    string Id,
    string Title,
    DateTime CreatedAt
);

public sealed record CharacterDashboardTheoryResponse(
    string Id,
    string Title,
    int Confidence,
    TheoryStatus Status
);

public sealed record CharacterDashboardOperationResponse(
    string Id,
    string Name,
    OperationStatus Status
);

public sealed record CharacterDashboardImportantPersonResponse(
    string Id,
    string Name,
    ImportantPersonType Type,
    EvaluationLevel TrustLevel,
    EvaluationLevel RiskLevel,
    EvaluationLevel UtilityLevel
);
