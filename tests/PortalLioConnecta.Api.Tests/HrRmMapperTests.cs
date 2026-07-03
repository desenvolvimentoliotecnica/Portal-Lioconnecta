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
