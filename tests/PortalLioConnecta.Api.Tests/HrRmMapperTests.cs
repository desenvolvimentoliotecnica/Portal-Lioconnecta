using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Tests;

public class HrRmMapperTests
{
    [Theory]
    [InlineData(2026, 5, "2026-05", "Maio/2026")]
    [InlineData(2025, 12, "2025-12", "Dezembro/2025")]
    public void BuildPeriodHelpers_ReturnExpectedValues(int year, int month, string id, string label)
    {
        Assert.Equal(id, HrRmMapper.BuildPeriodId(year, month));
        Assert.Equal(label, HrRmMapper.BuildPeriodLabel(year, month));
    }

    [Theory]
    [InlineData(2026, 6, 1, "2026-06")]
    [InlineData(2026, 6, 2, "2026-06-2")]
    public void BuildPayslipId_UsesPeriodSuffixWhenNeeded(int year, int month, int period, string expected)
    {
        Assert.Equal(expected, HrRmMapper.BuildPayslipId(year, month, period));
    }

    [Theory]
    [InlineData("2026-06", 2026, 6, null)]
    [InlineData("2026-06-2", 2026, 6, 2)]
    public void TryParsePayslipId_ParsesLegacyAndEnvelopeIds(string id, int year, int month, int? period)
    {
        Assert.True(HrRmMapper.TryParsePayslipId(id, out var parsedYear, out var parsedMonth, out var parsedPeriod));
        Assert.Equal(year, parsedYear);
        Assert.Equal(month, parsedMonth);
        Assert.Equal(period, parsedPeriod);
    }

    [Fact]
    public void MapPaymentTypeLabel_DetectsAdvanceEnvelope()
    {
        var advance = new RmPayslipSummaryRecord
        {
            NroPeriodo = 2,
            HasAdvanceEvent = true,
            HasPayrollEvents = false
        };

        Assert.Equal("ADIANTAMENTO", HrRmMapper.MapPaymentTypeLabel(advance));
    }

    [Fact]
    public void MapPaymentTypeLabel_DefaultsToFolha()
    {
        var folha = new RmPayslipSummaryRecord
        {
            NroPeriodo = 1,
            HasAdvanceEvent = false,
            HasPayrollEvents = true
        };

        Assert.Equal("FOLHA", HrRmMapper.MapPaymentTypeLabel(folha));
    }

    [Theory]
    [InlineData("M", "Gozado")]
    [InlineData("P", "Programado")]
    [InlineData("F", "Aprovado")]
    [InlineData("X", "Em analise")]
    public void MapVacationStatus_ReturnsFriendlyLabel(string code, string expected)
    {
        Assert.Equal(expected, HrRmMapper.MapVacationStatus(code));
    }

    [Fact]
    public void MaskCpf_HidesMiddleDigits()
    {
        Assert.Equal("***.456.789-**", HrRmMapper.MaskCpf("123.456.789-00"));
    }

    [Fact]
    public void FormatCityState_JoinsWhenBothPresent()
    {
        Assert.Equal("Sao Paulo / SP", HrRmMapper.FormatCityState("Sao Paulo", "SP"));
        Assert.Equal("—", HrRmMapper.FormatCityState(null, null));
    }
}
