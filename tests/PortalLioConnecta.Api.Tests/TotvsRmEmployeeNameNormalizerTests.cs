using PortalLioConnecta.Api.Infrastructure.TotvsRm;

namespace PortalLioConnecta.Api.Tests;

public class TotvsRmEmployeeNameNormalizerTests
{
    [Theory]
    [InlineData("Leonardo Sabino Mendes", "LEONARDO SABINO MENDES")]
    [InlineData("  Leonardo   Sabino Mendes  ", "LEONARDO SABINO MENDES")]
    [InlineData("Jose da Silva", "JOSE DA SILVA")]
    public void NormalizeForComparison_CollapsesSpacesAndRemovesAccents(string input, string expected)
    {
        var normalized = TotvsRmEmployeeNameNormalizer.NormalizeForComparison(input);

        Assert.Equal(expected, normalized);
    }
}
