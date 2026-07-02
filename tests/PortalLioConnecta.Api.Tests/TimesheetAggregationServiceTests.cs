using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Tests;

public class TimesheetAggregationServiceTests
{
    private readonly TimesheetAggregationService _service = new();

    [Fact]
    public void AggregateByDay_BuildsClockInAndClockOutFromOrderedPunches()
    {
        var punches = new List<RmPunchRecord>
        {
            new() { DataPonto = new DateTime(2026, 6, 23), BatidaMinutos = 482, Natureza = 0 },
            new() { DataPonto = new DateTime(2026, 6, 23), BatidaMinutos = 720, Natureza = 2 },
            new() { DataPonto = new DateTime(2026, 6, 23), BatidaMinutos = 780, Natureza = 1 },
            new() { DataPonto = new DateTime(2026, 6, 23), BatidaMinutos = 1051, Natureza = 3 }
        };

        var result = _service.AggregateByDay(punches);
        var day = result[new DateTime(2026, 6, 23)];

        Assert.Equal("08:02", TimesheetAggregationService.FormatClock(day.ClockInMinutes!.Value));
        Assert.Equal("17:31", TimesheetAggregationService.FormatClock(day.ClockOutMinutes!.Value));
        Assert.Equal(60, day.BreakMinutes);
        Assert.Equal("Regular", day.Status);
    }

    [Fact]
    public void MergeService_UsesProcessedDayTotalsWhenAvailable()
    {
        var mergeService = new TimesheetMergeService(_service);
        var dataDe = new DateTime(2026, 6, 1);
        var dataAte = new DateTime(2026, 6, 1);

        var punches = new List<RmPunchRecord>
        {
            new() { DataPonto = new DateTime(2026, 6, 1), BatidaMinutos = 480, Natureza = 0 },
            new() { DataPonto = new DateTime(2026, 6, 1), BatidaMinutos = 1020, Natureza = 2 }
        };

        var processed = new List<RmProcessedDayRecord>
        {
            new()
            {
                DataPonto = new DateTime(2026, 6, 1),
                WorkedMinutes = 480,
                ExpectedMinutes = 480,
                BalanceMinutes = 0,
                DelayMinutes = 0,
                AbsenceMinutes = 0,
                StatusCode = "D"
            }
        };

        var (summary, entries) = mergeService.Merge(dataDe, dataAte, punches, processed);

        Assert.Single(entries);
        Assert.Equal("8h00", entries[0].WorkedHours);
        Assert.Equal("0h00", entries[0].BalanceHours);
        Assert.Equal("Regular", entries[0].Status);
        Assert.Equal("8h00", summary.WorkedHours);
    }
}
