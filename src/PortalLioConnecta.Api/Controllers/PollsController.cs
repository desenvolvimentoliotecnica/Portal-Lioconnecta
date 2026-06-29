using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Polls;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly IPortalAuthService _portalAuthService;

    public PollsController(IPollService pollService, IPortalAuthService portalAuthService)
    {
        _pollService = pollService;
        _portalAuthService = portalAuthService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PollDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var items = await _pollService.GetPublishedAsync(portalUserId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var item = await _pollService.GetPublishedBySlugAsync(slug, portalUserId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id:guid}/vote")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vote(Guid id, [FromBody] SubmitPollVoteRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUserId == Guid.Empty || session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var item = await _pollService.SubmitVoteAsync(id, session.PortalUserId, request.OptionIds, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private async Task<Guid?> ResolvePortalUserIdAsync(CancellationToken cancellationToken)
    {
        var token = ResolvePortalToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var session = await _portalAuthService.GetActiveSessionEntityAsync(token, cancellationToken);
        return session?.PortalUserId;
    }

    private string ResolvePortalToken()
    {
        if (Request.Headers.TryGetValue("X-Portal-Token", out var portalToken))
        {
            return portalToken.ToString();
        }

        if (Request.Headers.Authorization.Count > 0)
        {
            var header = Request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return header[prefix.Length..].Trim();
            }
        }

        return string.Empty;
    }
}
