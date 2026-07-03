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
    [InlineData(2026, 6, 1, "FOLHA", false, "2026-06")]
    [InlineData(2026, 6, 2, "ADIANTAMENTO", false, "2026-06-ADIANTAMENTO")]
    [InlineData(2026, 6, 1, "ADIANTAMENTO", false, "2026-06-ADIANTAMENTO")]
    [InlineData(2026, 6, 1, "FOLHA", true, "2026-06-FOLHA")]
    [InlineData(2026, 6, 2, "ADIANTAMENTO", true, "2026-06-ADIANTAMENTO")]
    public void BuildPayslipId_UsesPeriodSuffixWhenNeeded(
        int year,
        int month,
        int period,
        string paymentType,
        bool multipleEnvelopes,
        string expected)
    {
        Assert.Equal(expected, HrRmMapper.BuildPayslipId(year, month, period, paymentType, multipleEnvelopes));
    }

    [Fact]
    public void FilterLinesByPaymentType_SplitsAdvanceAndFolhaLines()
    {
        var lines = new List<RmPayslipLineRecord>
        {
            new() { Code = "401", Description = "Adiantamento Normal Vencimento", Amount = 100m, IsDeduction = false },
            new() { Code = "001", Description = "Salario base", Amount = 500m, IsDeduction = false },
            new() { Code = "404", Description = "Adiantamento Normal Desconto", Amount = 100m, IsDeduction = true }
        };

        var advance = HrRmMapper.FilterLinesByPaymentType(lines, "ADIANTAMENTO");
        var folha = HrRmMapper.FilterLinesByPaymentType(lines, "FOLHA");

        Assert.Single(advance);
        Assert.Equal("401", advance[0].Code);
        Assert.Equal(2, folha.Count);
        Assert.Contains(folha, line => line.Code == "001");
        Assert.Contains(folha, line => line.Code == "404");
    }

    [Theory]
    [InlineData("2026-06", 2026, 6, null, null)]
    [InlineData("2026-06-2", 2026, 6, 2, null)]
    [InlineData("2026-06-FOLHA", 2026, 6, null, "FOLHA")]
    [InlineData("2026-06-ADIANTAMENTO", 2026, 6, null, "ADIANTAMENTO")]
    public void TryParsePayslipId_ParsesLegacyAndEnvelopeIds(
        string id,
        int year,
        int month,
        int? period,
        string? paymentTypeHint)
    {
        Assert.True(HrRmMapper.TryParsePayslipId(id, out var parsedYear, out var parsedMonth, out var parsedPeriod, out var parsedHint));
        Assert.Equal(year, parsedYear);
        Assert.Equal(month, parsedMonth);
        Assert.Equal(period, parsedPeriod);
        Assert.Equal(paymentTypeHint, parsedHint);
    }

    [Fact]
    public void MapPaymentTypeLabel_DetectsAdvanceWithoutDeductions()
    {
        var advance = new RmPayslipSummaryRecord
        {
            GrossAmount = 4644.58m,
            NetAmount = 4644.58m,
            DeductionAmount = 0m,
            PaymentDate = new DateTime(2026, 6, 15)
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
