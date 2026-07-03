using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Tests;

public class PortalUserEmployeeIdResolutionTests
{
    [Fact]
    public void BuildMissingProfileMessage_IncludesMatriculaWhenProvided()
    {
        var message = PortalUserEmployeeIdResolution.BuildMissingProfileMessage("00000581");

        Assert.Contains("(00000581)", message);
        Assert.Contains("nao esta vinculada ao perfil", message);
    }

    [Fact]
    public void BuildMissingProfileMessage_UsesPlaceholderWhenMissing()
    {
        var message = PortalUserEmployeeIdResolution.BuildMissingProfileMessage(null);

        Assert.Contains("(nao informada)", message);
    }
}
