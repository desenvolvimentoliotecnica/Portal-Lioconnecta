using PortalLioConnecta.Api.Infrastructure.TotvsRm;

namespace PortalLioConnecta.Api.Tests;

public class TotvsRmPeriodBankReaderTests
{
    [Fact]
    public void CalculateNetBalance_ReturnsExtraMinusDelayMinusAbsence()
    {
        var result = TotvsRmPeriodBankReader.CalculateNetBalance(1060, 0, 0);
        Assert.Equal(1060, result);
    }

    [Fact]
    public void CalculateNetBalance_MatchesRmEspelhoExample()
    {
        var previous = TotvsRmPeriodBankReader.CalculateNetBalance(1060, 0, 0);
        var period = TotvsRmPeriodBankReader.CalculateNetBalance(726, 0, 0);

        Assert.Equal(1060, previous);
        Assert.Equal(726, period);
        Assert.Equal(1786, previous + period);
    }

    [Fact]
    public void CalculateNetBalance_ReturnsNullWhenAllInputsMissing()
    {
        var result = TotvsRmPeriodBankReader.CalculateNetBalance(null, null, null);
        Assert.Null(result);
    }

    [Fact]
    public void CalculateNetBalance_TreatsNullComponentsAsZero()
    {
        var result = TotvsRmPeriodBankReader.CalculateNetBalance(100, null, 20);
        Assert.Equal(80, result);
    }
}
