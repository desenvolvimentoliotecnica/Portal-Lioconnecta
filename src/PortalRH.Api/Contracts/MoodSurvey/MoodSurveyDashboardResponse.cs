namespace PortalRH.Api.Contracts.MoodSurvey;

public sealed record MoodSurveyDashboardResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    string? Department,
    MoodSurveyDashboardSummaryDto Summary,
    IReadOnlyList<MoodSurveyOptionDistributionDto> Options,
    IReadOnlyList<MoodSurveyDepartmentBreakdownDto> Departments,
    IReadOnlyList<MoodSurveyDailyTrendDto> DailyTrend,
    IReadOnlyList<MoodSurveyDepartmentFilterOptionDto> DepartmentOptions);

public sealed record MoodSurveyDashboardSummaryDto(
    int TotalVotes,
    int UniqueUsers,
    int ActiveUsers,
    int MotivatedCount,
    int GoodCount,
    int TiredCount,
    decimal ParticipationRate);

public sealed record MoodSurveyOptionDistributionDto(
    string Key,
    string Label,
    string Emoji,
    int Count,
    decimal Percentage);

public sealed record MoodSurveyDepartmentBreakdownDto(
    string Department,
    int TotalVotes,
    int MotivatedCount,
    int GoodCount,
    int TiredCount,
    IReadOnlyList<MoodSurveyOptionDistributionDto> Options);

public sealed record MoodSurveyDailyTrendDto(
    DateOnly Date,
    int TotalVotes,
    int MotivatedCount,
    int GoodCount,
    int TiredCount);

public sealed record MoodSurveyDepartmentFilterOptionDto(
    string Key,
    string Label,
    int Count);
