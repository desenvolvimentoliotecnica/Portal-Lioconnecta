using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;
using System.Globalization;

namespace PortalLioConnecta.Api.Services;

public sealed class TimesheetAggregationService
{
    private static readonly HashSet<int> EntryNatureCodes = new() { 0, 1 };
    private static readonly HashSet<int> ExitNatureCodes = new() { 2, 3 };

    public IReadOnlyDictionary<DateTime, AggregatedDayPunch> AggregateByDay(IEnumerable<RmPunchRecord> punches)
    {
        return punches
            .GroupBy(item => item.DataPonto.Date)
            .ToDictionary(
                group => group.Key,
                group => AggregateDay(group.Key, group.OrderBy(item => item.BatidaMinutos).ToList()));
    }

    private static AggregatedDayPunch AggregateDay(DateTime date, IReadOnlyList<RmPunchRecord> dayPunches)
    {
        if (dayPunches.Count == 0)
        {
            return new AggregatedDayPunch(date, null, null, 0, 0, "Sem registro");
        }

        var entryCandidates = dayPunches
            .Where(item => EntryNatureCodes.Contains(item.Natureza)
                || ContainsKeyword(item.DescricaoNatureza, "entrada"))
            .Select(item => item.BatidaMinutos)
            .ToList();

        var exitCandidates = dayPunches
            .Where(item => ExitNatureCodes.Contains(item.Natureza)
                || ContainsKeyword(item.DescricaoNatureza, "saida"))
            .Select(item => item.BatidaMinutos)
            .ToList();

        var ordered = dayPunches.Select(item => item.BatidaMinutos).OrderBy(item => item).ToList();
        var clockIn = entryCandidates.Count > 0 ? entryCandidates.Min() : ordered.First();
        var clockOut = exitCandidates.Count > 0 ? exitCandidates.Max() : ordered.Last();

        var breakMinutes = CalculateBreakMinutes(ordered, clockIn, clockOut);
        var workedMinutes = Math.Max(0, clockOut - clockIn - breakMinutes);
        var status = dayPunches.Count >= 2 && clockOut > clockIn ? "Regular" : "Incompleto";

        return new AggregatedDayPunch(
            date,
            clockIn,
            clockOut,
            breakMinutes,
            workedMinutes,
            status);
    }

    private static int CalculateBreakMinutes(IReadOnlyList<int> ordered, int clockIn, int clockOut)
    {
        if (ordered.Count < 4)
        {
            return 0;
        }

        var breakMinutes = 0;
        for (var index = 1; index < ordered.Count - 1; index += 2)
        {
            var exitMinute = ordered[index];
            var nextEntryMinute = ordered[index + 1];
            if (exitMinute >= clockIn && nextEntryMinute <= clockOut && nextEntryMinute > exitMinute)
            {
                breakMinutes += nextEntryMinute - exitMinute;
            }
        }

        return breakMinutes;
    }

    private static bool ContainsKeyword(string? description, string keyword)
    {
        return !string.IsNullOrWhiteSpace(description)
            && description.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    public static string FormatMinutes(int minutes)
    {
        var absolute = Math.Abs(minutes);
        var hours = absolute / 60;
        var mins = absolute % 60;
        var formatted = $"{hours}h{mins:D2}";
        return minutes < 0 ? $"-{formatted}" : formatted;
    }

    public static string FormatClock(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }

    public static string GetWeekdayLabel(DateTime date)
    {
        return CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetDayName(date.DayOfWeek);
    }
}

public sealed record AggregatedDayPunch(
    DateTime Date,
    int? ClockInMinutes,
    int? ClockOutMinutes,
    int BreakMinutes,
    int WorkedMinutes,
    string Status);
