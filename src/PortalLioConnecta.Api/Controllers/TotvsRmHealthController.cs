using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("health")]
public class TotvsRmHealthController : ControllerBase
{
    private readonly ITotvsRmConfigurationService _configurationService;
    private readonly TotvsRmConnectionTester _connectionTester;

    public TotvsRmHealthController(
        ITotvsRmConfigurationService configurationService,
        TotvsRmConnectionTester connectionTester)
    {
        _configurationService = configurationService;
        _connectionTester = connectionTester;
    }

    [HttpGet("rm")]
    public async Task<IActionResult> GetRmHealth(CancellationToken cancellationToken)
    {
        var runtime = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!runtime.IsEnabled)
        {
            return Ok(new { status = "disabled", message = "Integracao TOTVS RM desabilitada." });
        }

        var test = await _connectionTester.TestAsync(runtime, cancellationToken);
        return Ok(new
        {
            status = test.Success ? "healthy" : "unhealthy",
            message = test.Message,
            detail = test.Detail,
            codColigada = runtime.CodColigada,
            modules = runtime.ModuleFlags
        });
    }
}
