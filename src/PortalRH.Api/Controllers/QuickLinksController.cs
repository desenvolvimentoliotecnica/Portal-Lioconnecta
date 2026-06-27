using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.QuickLinks;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

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
