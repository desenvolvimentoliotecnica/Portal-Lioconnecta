using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Shell;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

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
