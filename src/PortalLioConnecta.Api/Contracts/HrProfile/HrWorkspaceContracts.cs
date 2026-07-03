namespace PortalLioConnecta.Api.Contracts.HrProfile;

public sealed record HrAvailabilityFields(
    string? AvailabilityStatus = null,
    string? UserMessage = null);

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
    HrVacationBalanceDto? Balance,
    IReadOnlyList<HrVacationRequestDto> Requests,
    bool CanRequest,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

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
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

public sealed record HrPayslipLineDto(
    string Code,
    string Description,
    string Reference,
    decimal Amount);

public sealed record HrPayslipDetailDto(
    string Id,
    string PeriodLabel,
    string ReferenceMonth,
    decimal GrossAmount,
    decimal NetAmount,
    DateTime PaymentDate,
    string Status,
    string CompanyName,
    string CompanyCnpj,
    string CompanyAddress,
    string EmployeeName,
    string EmployeeRegistration,
    string EmployeeCpf,
    string EmployeeRole,
    string EmployeeDepartment,
    string EmployeeAdmissionDate,
    string BankName,
    string BankAgency,
    string BankAccount,
    decimal BaseSalary,
    decimal BaseInss,
    decimal BaseFgts,
    decimal FgtsAmount,
    IReadOnlyList<HrPayslipLineDto> Earnings,
    IReadOnlyList<HrPayslipLineDto> Deductions,
    decimal TotalEarnings,
    decimal TotalDeductions,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

public sealed record HrBenefitItemDto(
    string Code,
    string Label,
    string Category,
    string Value,
    string Status,
    string Details);

public sealed record HrDependentItemDto(
    string Name,
    string Relationship,
    string Status);

public sealed record HrBenefitsResponse(
    string Title,
    IReadOnlyList<HrBenefitItemDto> Items,
    IReadOnlyList<HrDependentItemDto> Dependents,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

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
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

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
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

public sealed record HrTimesheetPeriodOptionDto(
    int EndMonth,
    int EndYear,
    string Label);

public sealed record HrTimesheetEntryDto(
    DateTime Date,
    string WeekdayLabel,
    string ClockIn,
    string LunchOut,
    string LunchIn,
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
    int Delays,
    string? BankHours = null);

public sealed record HrTimesheetResponse(
    string Title,
    HrTimesheetSummaryDto? Summary,
    IReadOnlyList<HrTimesheetEntryDto> Entries,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus,
    string? UserMessage,
    int SelectedPeriodEndMonth = 0,
    int SelectedPeriodEndYear = 0,
    IReadOnlyList<HrTimesheetPeriodOptionDto>? PeriodOptions = null);

public sealed record HrRhSummaryDto(
    string? VacationBalanceDays,
    string? LastPayslipNet,
    string? LastPayslipPeriod,
    string? MonthlyWorkedHours,
    string? MonthlyBalanceHours,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);

public sealed record HrTeamMemberDto(
    string Chapa,
    string Name,
    string Role,
    string Department,
    string Status);

public sealed record HrTeamDashboardResponse(
    string Title,
    IReadOnlyList<HrTeamMemberDto> Members,
    int ActiveCount,
    string Provider,
    bool IsSimulated,
    string? AvailabilityStatus = null,
    string? UserMessage = null);
