using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Shell;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/panels")]
[RequirePortalSession]
public class PanelsController : ControllerBase
{
    private readonly IPortalShellService _portalShellService;

    public PanelsController(IPortalShellService portalShellService)
    {
        _portalShellService = portalShellService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PanelsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _portalShellService.BuildPanelsAsync(session.PortalUser, cancellationToken);
        return Ok(payload);
    }
}
