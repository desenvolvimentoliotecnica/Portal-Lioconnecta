namespace PortalLioConnecta.Api.Contracts.Journey;

public sealed record JourneyTasksSummaryDto(
    int OpenCount,
    int OverdueCount,
    int DueTodayCount);

public sealed record JourneyTaskItemDto(
    Guid Id,
    string TypeKey,
    string TypeLabel,
    string Title,
    string Priority,
    DateTime DueDate,
    string Status,
    string Assignee,
    string? Description,
    DateTime CreatedAtUtc,
    bool IsUserCreated);

public sealed record JourneyTasksResponse(
    string Title,
    JourneyTasksSummaryDto Summary,
    IReadOnlyList<JourneyTaskItemDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record JourneyCreateTaskDto(
    string TypeKey,
    string Title,
    string Description,
    string Priority,
    DateTime DueDate,
    IReadOnlyDictionary<string, string>? Fields);

public sealed record JourneyUpdateTaskDto(
    string Title,
    string Description,
    string Priority,
    DateTime DueDate,
    IReadOnlyDictionary<string, string>? Fields);

public sealed record JourneyUpdateTaskStatusDto(
    string Status);

public sealed record JourneyCreateTaskResponse(
    JourneyTaskItemDto Item,
    JourneyTasksSummaryDto Summary,
    string Provider,
    bool IsSimulated);

public sealed record JourneyUpdateTaskResponse(
    JourneyTaskItemDto Item,
    JourneyTasksSummaryDto Summary,
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

public sealed record JourneyLearningCourseDto(
    Guid Id,
    string Title,
    string Description,
    string DurationLabel,
    string Format,
    string Status,
    string? TrilhaTitle);

public sealed record JourneyLearningMaterialDto(
    Guid Id,
    string Title,
    string Type,
    string SizeLabel,
    string Status);

public sealed record JourneyLearningCatalogSummaryDto(
    int CoursesCount,
    int MaterialsCount,
    string HoursLabel);

public sealed record JourneyLearningCatalogResponse(
    string Title,
    JourneyLearningCatalogSummaryDto Summary,
    IReadOnlyList<JourneyLearningCourseDto> Courses,
    IReadOnlyList<JourneyLearningMaterialDto> Materials,
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

public sealed record JourneyCreateRequestDto(
    string TypeKey,
    string Subject,
    string Description,
    string Priority,
    IReadOnlyDictionary<string, string>? Fields);

public sealed record JourneyCreateRequestResponse(
    JourneyRequestItemDto Item,
    JourneyRequestsSummaryDto Summary,
    string Provider,
    bool IsSimulated);
