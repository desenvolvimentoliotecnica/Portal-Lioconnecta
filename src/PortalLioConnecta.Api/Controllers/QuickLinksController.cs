using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.QuickLinks;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/quick-links")]
[RequirePortalSession]
public class QuickLinksController : ControllerBase
{
    private readonly IQuickLinkService _quickLinkService;

    public QuickLinksController(IQuickLinkService quickLinkService)
    {
        _quickLinkService = quickLinkService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(QuickLinkListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var payload = await _quickLinkService.GetActiveAsync(cancellationToken);
        return Ok(payload);
    }
}
