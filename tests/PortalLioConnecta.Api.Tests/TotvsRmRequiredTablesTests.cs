using PortalLioConnecta.Api.Infrastructure.TotvsRm;

namespace PortalLioConnecta.Api.Tests;

public class TotvsRmRequiredTablesTests
{
    [Fact]
    public void RequiredReadTables_ContainsAllPortalModules()
    {
        var required = TotvsRmConstants.RequiredReadTables;

        Assert.Contains("PFUNC", required);
        Assert.Contains("PPESSOA", required);
        Assert.Contains("PFFINANC", required);
        Assert.Contains("PEVENTO", required);
        Assert.Contains("PFUFERIAS", required);
        Assert.Contains("PFUFERIASPER", required);
        Assert.Contains("PFDEPEND", required);
        Assert.Contains("ABATFUN", required);
        Assert.Contains("AAFHTFUN", required);
        Assert.Contains("ANATUBAT", required);
        Assert.Contains("ACOMPFUN", required);
        Assert.Equal(13, required.Length);
    }
}
