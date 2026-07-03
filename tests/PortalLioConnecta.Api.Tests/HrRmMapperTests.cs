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
            new() { Code = "401", Description = "Adiantamento Normal Vencimento", Amount = 100m },
            new() { Code = "001", Description = "Salario base", Amount = 500m },
            new() { Code = "404", Description = "Adiantamento Normal Desconto", Amount = 100m, IsDeduction = true }
        };

        var advance = HrRmMapper.FilterLinesByPaymentType(lines, "ADIANTAMENTO");
        var folha = HrRmMapper.FilterLinesByPaymentType(lines, "FOLHA");

        Assert.Single(advance);
        Assert.Equal("401", advance[0].Code);
        Assert.Equal(2, folha.Count);
    }

    [Fact]
    public void FilterLinesByPaymentType_FolhaMatchesRmAppDisplayLines()
    {
        var lines = new List<RmPayslipLineRecord>
        {
            new() { Code = "0092", Description = "INSS com Aliquota Normal", Amount = 988.07m, ProvisionType = "P" },
            new() { Code = "288", Description = "BS EM Refeicao Empresa", Amount = 591.73m, ProvisionType = "P" },
            new() { Code = "9999", Description = "HORAS TRABALHADAS CHEIA", Amount = 11611.45m, ProvisionType = "P" },
            new() { Code = "0001", Description = "Horas Trabalhadas", Amount = 9676.03m, ProvisionType = "P" },
            new() { Code = "0002", Description = "DSR Horas Trabalhadas", Amount = 1935.42m, ProvisionType = "P" },
            new() { Code = "101", Description = "Hrs Extras Diurnas 60%", Amount = 63.34m, ProvisionType = "P" },
            new() { Code = "37", Description = "Repouso Remunerado Adicionais", Amount = 12.67m, ProvisionType = "P" },
            new() { Code = "511", Description = "INSS Normal", Amount = 988.07m, IsDeduction = true, ProvisionType = "D" }
        };

        var folha = HrRmMapper.FilterLinesByPaymentType(lines, "FOLHA");

        Assert.Equal(4, folha.Count(line => !line.IsDeduction));
        Assert.Single(folha, line => line.IsDeduction);
        Assert.Equal(11687.46m, folha.Where(line => !line.IsDeduction).Sum(line => line.Amount));
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

    [Fact]
    public void ResolveFgtsAmount_UsesStoredValueWhenPresent()
    {
        Assert.Equal(934.99m, HrRmMapper.ResolveFgtsAmount(11687.46m, 934.99m));
    }

    [Fact]
    public void ResolveFgtsAmount_DerivesEightPercentWhenMissing()
    {
        Assert.Equal(934.99m, HrRmMapper.ResolveFgtsAmount(11687.46m, 0m));
    }

    [Fact]
    public void ResolveFgtsAmount_ReturnsZeroWithoutBase()
    {
        Assert.Equal(0m, HrRmMapper.ResolveFgtsAmount(0m, 0m));
    }
}
