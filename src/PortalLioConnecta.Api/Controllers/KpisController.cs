using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Kpis;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/kpis")]
[RequirePortalSession]
public class KpisController : ControllerBase
{
    private readonly IKpiService _kpiService;

    public KpisController(IKpiService kpiService)
    {
        _kpiService = kpiService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(KpiSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _kpiService.GetSummaryAsync(session.PortalUser, cancellationToken);
        return Ok(payload);
    }
}
