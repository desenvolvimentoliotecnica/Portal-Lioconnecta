namespace PortalRH.Api.Contracts.Journey;

public sealed record JourneyTasksSummaryDto(
    int OpenCount,
    int OverdueCount,
    int DueTodayCount);

public sealed record JourneyTaskItemDto(
    Guid Id,
    string Title,
    string Priority,
    DateTime DueDate,
    string Status,
    string Assignee);

public sealed record JourneyTasksResponse(
    string Title,
    JourneyTasksSummaryDto Summary,
    IReadOnlyList<JourneyTaskItemDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record JourneyRequestsSummaryDto(
    int TotalCount,
    int PendingApprovalCount,
    int InProgressCount);

public sealed record JourneyRequestItemDto(
    Guid Id,
    string Type,
    string Description,
    DateTime OpenedAtUtc,
    string Status,
    string Stage);

public sealed record JourneyRequestsResponse(
    string Title,
    JourneyRequestsSummaryDto Summary,
    IReadOnlyList<JourneyRequestItemDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record JourneyLearningPathsSummaryDto(
    int EnrolledCount,
    int CompletedCount,
    string HoursLabel);

public sealed record JourneyLearningPathItemDto(
    Guid Id,
    string Title,
    int ProgressPercent,
    DateTime? DueDate,
    string Status,
    string DurationLabel);

public sealed record JourneyLearningPathsResponse(
    string Title,
    JourneyLearningPathsSummaryDto Summary,
    IReadOnlyList<JourneyLearningPathItemDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record JourneyDocumentItemDto(
    Guid Id,
    string Title,
    string Category,
    DateTime UpdatedAtUtc,
    string SizeLabel,
    string Status);

public sealed record JourneyDocumentsResponse(
    string Title,
    IReadOnlyList<JourneyDocumentItemDto> Items,
    string Provider,
    bool IsSimulated);
