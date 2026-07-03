using PortalLioConnecta.Api.Contracts.HrProfile;
using PortalLioConnecta.Api.Infrastructure;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;
using System.Globalization;

namespace PortalLioConnecta.Api.Services;

public sealed class TimesheetMergeService
{
    private readonly TimesheetAggregationService _aggregationService;

    public TimesheetMergeService(TimesheetAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    public (HrTimesheetSummaryDto Summary, IReadOnlyList<HrTimesheetEntryDto> Entries) Merge(
        DateTime dataDe,
        DateTime dataAte,
        IReadOnlyList<RmPunchRecord> punches,
        IReadOnlyList<RmProcessedDayRecord> processedDays)
    {
        var aggregated = _aggregationService.AggregateByDay(punches);
        var processedByDate = processedDays.ToDictionary(item => item.DataPonto.Date);

        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone.SaoPauloTimeZone).Date;
        var effectiveEnd = dataAte.Date > todayLocal ? todayLocal : dataAte.Date;
        if (effectiveEnd < dataDe.Date)
        {
            effectiveEnd = dataDe.Date;
        }

        var dates = Enumerable
            .Range(0, (effectiveEnd - dataDe.Date).Days + 1)
            .Select(offset => dataDe.Date.AddDays(offset))
            .OrderByDescending(item => item)
            .ToList();

        var entries = dates
            .Select(date => BuildEntry(date, aggregated, processedByDate))
            .ToList();

        var summary = BuildSummary(dataDe, dataAte, entries, processedDays);
        return (summary, entries);
    }

    private HrTimesheetEntryDto BuildEntry(
        DateTime date,
        IReadOnlyDictionary<DateTime, AggregatedDayPunch> aggregated,
        IReadOnlyDictionary<DateTime, RmProcessedDayRecord> processedByDate)
    {
        aggregated.TryGetValue(date, out var dayPunch);
        processedByDate.TryGetValue(date, out var processed);

        var clockIn = FormatPunch(dayPunch?.ClockInMinutes);
        var lunchOut = FormatPunch(dayPunch?.LunchOutMinutes);
        var lunchIn = FormatPunch(dayPunch?.LunchInMinutes);
        var clockOut = FormatPunch(dayPunch?.ClockOutMinutes);

        var breakMinutes = dayPunch?.BreakMinutes ?? 0;
        var workedMinutes = processed?.WorkedMinutes ?? dayPunch?.WorkedMinutes ?? 0;
        var expectedMinutes = processed?.ExpectedMinutes;
        var balanceMinutes = processed?.BalanceMinutes
            ?? (expectedMinutes.HasValue ? workedMinutes - expectedMinutes.Value : (int?)null);

        var status = ResolveStatus(processed, dayPunch);

        return new HrTimesheetEntryDto(
            date,
            TimesheetAggregationService.GetWeekdayLabel(date),
            clockIn,
            lunchOut,
            lunchIn,
            clockOut,
            breakMinutes.ToString(CultureInfo.InvariantCulture),
            TimesheetAggregationService.FormatMinutes(workedMinutes),
            balanceMinutes.HasValue
                ? FormatSignedMinutes(balanceMinutes.Value)
                : "—",
            status);
    }

    private static string FormatPunch(int? minutes)
    {
        return minutes is int value
            ? TimesheetAggregationService.FormatClock(value)
            : "—";
    }

    private static string ResolveStatus(RmProcessedDayRecord? processed, AggregatedDayPunch? dayPunch)
    {
        if (!string.IsNullOrWhiteSpace(processed?.StatusCode))
        {
            return MapRmStatus(processed.StatusCode);
        }

        return dayPunch?.Status ?? "Sem registro";
    }

    private static string MapRmStatus(string statusCode)
    {
        return statusCode.Trim().ToUpperInvariant() switch
        {
            "F" => "Falta",
            "A" => "Atraso",
            "D" => "Regular",
            _ => statusCode
        };
    }

    private HrTimesheetSummaryDto BuildSummary(
        DateTime dataDe,
        DateTime dataAte,
        IReadOnlyList<HrTimesheetEntryDto> entries,
        IReadOnlyList<RmProcessedDayRecord> processedDays)
    {
        var periodLabel = TimesheetPeriodResolver.FormatPeriodLabel(dataDe.Date, dataAte.Date);

        var absences = processedDays.Count(item => (item.AbsenceMinutes ?? 0) > 0
            || string.Equals(item.StatusCode, "F", StringComparison.OrdinalIgnoreCase));
        var delays = processedDays.Count(item => (item.DelayMinutes ?? 0) > 0
            || string.Equals(item.StatusCode, "A", StringComparison.OrdinalIgnoreCase));

        return new HrTimesheetSummaryDto(
            periodLabel,
            "—",
            "—",
            "—",
            absences,
            delays);
    }

    private static string FormatSignedMinutes(int minutes)
    {
        var formatted = TimesheetAggregationService.FormatMinutes(minutes);
        return minutes > 0 ? $"+{formatted}" : formatted;
    }
}
