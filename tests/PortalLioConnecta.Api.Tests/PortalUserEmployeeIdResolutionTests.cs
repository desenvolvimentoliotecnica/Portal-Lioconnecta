using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Services;
using System.Text.Json;

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

    [Fact]
    public void MapGraphProfile_UsesEmployeeNumberWhenEmployeeIdMissing()
    {
        using var document = JsonDocument.Parse("""
            {
              "displayName": "Leonardo Sabino Mendes",
              "employeeNumber": "00000581"
            }
            """);

        var employeeId = PortalUserEmployeeIdResolver.TryReadEmployeeIdFromGraph(document.RootElement);

        Assert.Equal("00000581", employeeId);
    }

    [Fact]
    public void TryReadEmployeeIdFromGraph_UsesOnPremisesExtensionAttribute()
    {
        using var document = JsonDocument.Parse("""
            {
              "displayName": "Leonardo Sabino Mendes",
              "onPremisesExtensionAttributes": {
                "extensionAttribute1": "00000581"
              }
            }
            """);

        var employeeId = PortalUserEmployeeIdResolver.TryReadEmployeeIdFromGraph(document.RootElement);

        Assert.Equal("00000581", employeeId);
    }
}
