using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/hr/profile")]
[RequirePortalSession]
public class HrProfileController : ControllerBase
{
    private readonly IHrProfileService _hrProfileService;

    public HrProfileController(IHrProfileService hrProfileService)
    {
        _hrProfileService = hrProfileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HrProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _hrProfileService.GetProfileAsync(session.PortalUser, cancellationToken);
        return Ok(payload);
    }
}
