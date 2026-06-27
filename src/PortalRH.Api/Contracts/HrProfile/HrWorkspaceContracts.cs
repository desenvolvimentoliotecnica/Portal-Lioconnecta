namespace PortalRH.Api.Contracts.HrProfile;

public sealed record HrVacationBalanceDto(
    int AvailableDays,
    int ScheduledDays,
    int UsedDays,
    DateTime? NextAcquisitionDate);

public sealed record HrVacationRequestDto(
    Guid Id,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    int Days,
    DateTime RequestedAtUtc);

public sealed record HrVacationResponse(
    string Title,
    HrVacationBalanceDto Balance,
    IReadOnlyList<HrVacationRequestDto> Requests,
    bool CanRequest,
    string Provider,
    bool IsSimulated);

public sealed record HrPayslipDto(
    string Id,
    string PeriodLabel,
    string ReferenceMonth,
    decimal GrossAmount,
    decimal NetAmount,
    DateTime PaymentDate,
    string Status);

public sealed record HrPayslipResponse(
    string Title,
    IReadOnlyList<HrPayslipDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record HrBenefitItemDto(
    string Code,
    string Label,
    string Category,
    string Value,
    string Status,
    string Details);

public sealed record HrBenefitsResponse(
    string Title,
    IReadOnlyList<HrBenefitItemDto> Items,
    string Provider,
    bool IsSimulated);

public sealed record HrEvaluationCompetencyDto(
    string Name,
    int Score,
    int MaxScore,
    string LevelLabel);

public sealed record HrEvaluationResponse(
    string Title,
    string CycleLabel,
    string Status,
    decimal OverallScore,
    string OverallLabel,
    IReadOnlyList<HrEvaluationCompetencyDto> Competencies,
    string ManagerFeedback,
    string Provider,
    bool IsSimulated);

public sealed record HrPersonalDataFieldDto(
    string Label,
    string Value,
    bool IsEditable);

public sealed record HrPersonalDataSectionDto(
    string Title,
    IReadOnlyList<HrPersonalDataFieldDto> Fields);

public sealed record HrPersonalDataResponse(
    string Title,
    IReadOnlyList<HrPersonalDataSectionDto> Sections,
    string Provider,
    bool IsSimulated);

public sealed record HrTimesheetEntryDto(
    DateTime Date,
    string WeekdayLabel,
    string ClockIn,
    string ClockOut,
    string BreakMinutes,
    string WorkedHours,
    string BalanceHours,
    string Status);

public sealed record HrTimesheetSummaryDto(
    string PeriodLabel,
    string WorkedHours,
    string ExpectedHours,
    string BalanceHours,
    int Absences,
    int Delays);

public sealed record HrTimesheetResponse(
    string Title,
    HrTimesheetSummaryDto Summary,
    IReadOnlyList<HrTimesheetEntryDto> Entries,
    string Provider,
    bool IsSimulated);
