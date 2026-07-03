using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Tests;

public class TotvsRmModuleFlagsTests
{
    [Fact]
    public void AllEnabled_HasEveryModuleOn()
    {
        var flags = TotvsRmModuleFlags.AllEnabled;

        Assert.True(flags.Cadastro);
        Assert.True(flags.Holerite);
        Assert.True(flags.Ferias);
        Assert.True(flags.Beneficios);
        Assert.True(flags.Ponto);
        Assert.True(flags.TeamDashboard);
        Assert.True(flags.HasAnyEnabled);
    }

    [Fact]
    public void LegacyUnsetFlags_HasAnyEnabledIsFalse()
    {
        var flags = new TotvsRmModuleFlags(false, false, false, false, false, false);

        Assert.False(flags.HasAnyEnabled);
    }

    [Fact]
    public void HrResolutionFactories_SetExpectedStatuses()
    {
        var context = new TotvsRmHrContext("00001234", 10, "01.01");

        var ok = TotvsRmHrResolution.Ok(context, "ldap");
        Assert.True(ok.IsSuccess);
        Assert.Equal("ok", ok.AvailabilityStatus);

        var disabled = TotvsRmHrResolution.Disabled("off");
        Assert.Equal("rm_disabled", disabled.AvailabilityStatus);

        var unavailable = TotvsRmHrResolution.Unavailable("down");
        Assert.Equal("rm_unavailable", unavailable.AvailabilityStatus);
    }
}
