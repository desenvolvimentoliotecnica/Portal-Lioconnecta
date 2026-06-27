using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Communications;
using PortalRH.Api.Features.Communications.Commands.CreateCommunication;
using PortalRH.Api.Features.Communications.Commands.DeleteCommunication;
using PortalRH.Api.Features.Communications.Commands.UpdateCommunication;
using PortalRH.Api.Features.Communications.Queries.GetCommunicationById;
using PortalRH.Api.Features.Communications.Queries.GetCommunicationBySlug;
using PortalRH.Api.Features.Communications.Queries.GetCommunications;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICommunicationService _communicationService;
    private readonly IPortalAuthService _portalAuthService;

    public CommunicationsController(
        IMediator mediator,
        ICommunicationService communicationService,
        IPortalAuthService portalAuthService)
    {
        _mediator = mediator;
        _communicationService = communicationService;
        _portalAuthService = portalAuthService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CommunicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var result = await _mediator.Send(new GetCommunicationsQuery(portalUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommunicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var result = await _mediator.Send(new GetCommunicationByIdQuery(id, portalUserId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(CommunicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var result = await _mediator.Send(new GetCommunicationBySlugQuery(slug, portalUserId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/like")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(CommunicationLikeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLike(Guid id, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var result = await _communicationService.ToggleLikeAsync(
                id,
                session.PortalUserId,
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost]
    [RequireCommunicationEditor]
    [ProducesResponseType(typeof(CommunicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] UpsertCommunicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCommunicationCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequireCommunicationEditor]
    [ProducesResponseType(typeof(CommunicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertCommunicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCommunicationCommand(id, request), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequireCommunicationEditor]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteCommunicationCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
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

    private CommunicationAuditContext BuildAuditContext(string actorLogin, string actorDisplayName)
    {
        string? ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Contains(','))
        {
            ipAddress = ipAddress.Split(',')[0].Trim();
        }

        var origin = Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(origin) && Request.Host.HasValue)
        {
            origin = $"{Request.Scheme}://{Request.Host}";
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        return new CommunicationAuditContext(actorLogin, actorDisplayName, ipAddress, origin, userAgent);
    }
}
