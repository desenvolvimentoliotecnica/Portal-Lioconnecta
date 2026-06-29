using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Shell;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/me-ui")]
[RequirePortalSession]
public class MeUiController : ControllerBase
{
    private readonly IPortalShellService _portalShellService;

    public MeUiController(IPortalShellService portalShellService)
    {
        _portalShellService = portalShellService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MeUiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        return Ok(await _portalShellService.BuildMeUiAsync(session.PortalUser, cancellationToken));
    }
}
