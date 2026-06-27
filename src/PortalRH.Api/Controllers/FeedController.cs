using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Feed;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/feed")]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;
    private readonly IPortalAuthService _portalAuthService;

    public FeedController(IFeedService feedService, IPortalAuthService portalAuthService)
    {
        _feedService = feedService;
        _portalAuthService = portalAuthService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(FeedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed(CancellationToken cancellationToken)
    {
        var portalUserId = await ResolvePortalUserIdAsync(cancellationToken);
        var payload = await _feedService.GetFeedAsync(portalUserId, cancellationToken);
        return Ok(payload);
    }

    [HttpGet("saved")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(FeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSavedFeed(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _feedService.GetSavedFeedAsync(session.PortalUserId, cancellationToken);
        return Ok(payload);
    }

    [HttpPost]
    [RequirePortalSession]
    [ProducesResponseType(typeof(CreateFeedPostResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePost([FromBody] CreateFeedPostRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var item = await _feedService.CreatePostAsync(
                session.PortalUserId,
                request.Text,
                request.Media ?? [],
                request.MentionedUserIds ?? [],
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return Created("/api/feed", new CreateFeedPostResponse(item));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("mentions/suggest")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(FeedMentionSuggestionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestMentions([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var result = await _feedService.SuggestMentionsAsync(q ?? string.Empty, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(FeedPostCommentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostComments(Guid id, CancellationToken cancellationToken)
    {
        var result = await _feedService.GetPostCommentsAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(CreateFeedPostCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePostComment(
        Guid id,
        [FromBody] CreateFeedPostCommentRequest request,
        CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.HasModuleAccess(
                session.PortalUser,
                PortalModulePermissionCatalog.Feed,
                PortalModulePermissionCatalog.Interact,
                PortalModulePermissionCatalog.Manage))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para comentar no feed."
            });
        }

        try
        {
            var item = await _feedService.CreatePostCommentAsync(
                id,
                session.PortalUserId,
                request.Text,
                request.MentionedUserIds ?? [],
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return item is null
                ? NotFound()
                : Created($"/api/feed/{id}/comments", new CreateFeedPostCommentResponse(item));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/like")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(FeedLikeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLike(Guid id, [FromBody] ToggleFeedLikeRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var result = await _feedService.ToggleLikeAsync(
                id,
                request.Source,
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

    [HttpPost("{id:guid}/share")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(FeedShareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleShare(Guid id, [FromBody] ToggleFeedShareRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.HasModuleAccess(
                session.PortalUser,
                PortalModulePermissionCatalog.Feed,
                PortalModulePermissionCatalog.Interact,
                PortalModulePermissionCatalog.Manage))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para compartilhar no feed."
            });
        }

        try
        {
            var result = await _feedService.ToggleShareAsync(
                id,
                request.Source,
                session.PortalUserId,
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/save")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(FeedSaveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleSave(Guid id, [FromBody] ToggleFeedSaveRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var result = await _feedService.ToggleSaveAsync(
                id,
                request.Source,
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

    [HttpDelete("{id:guid}")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(DeleteFeedPostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.HasModuleAccess(
                session.PortalUser,
                PortalModulePermissionCatalog.Feed,
                PortalModulePermissionCatalog.Interact,
                PortalModulePermissionCatalog.Manage))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para remover publicacoes no feed."
            });
        }

        try
        {
            var deleted = await _feedService.DeletePostAsync(
                id,
                session.PortalUserId,
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return deleted
                ? Ok(new DeleteFeedPostResponse(id, true))
                : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    [HttpGet("media/{mediaId:guid}/comments")]
    [ProducesResponseType(typeof(FeedMediaCommentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMediaComments(Guid mediaId, CancellationToken cancellationToken)
    {
        var result = await _feedService.GetMediaCommentsAsync(mediaId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("media/{mediaId:guid}/comments")]
    [RequirePortalSession]
    [ProducesResponseType(typeof(CreateFeedMediaCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMediaComment(
        Guid mediaId,
        [FromBody] CreateFeedMediaCommentRequest request,
        CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.HasModuleAccess(
                session.PortalUser,
                PortalModulePermissionCatalog.Feed,
                PortalModulePermissionCatalog.Interact,
                PortalModulePermissionCatalog.Manage))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para comentar fotos no feed."
            });
        }

        try
        {
            var item = await _feedService.CreateMediaCommentAsync(
                mediaId,
                session.PortalUserId,
                request.Text,
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return item is null
                ? NotFound()
                : Created($"/api/feed/media/{mediaId}/comments", new CreateFeedMediaCommentResponse(item));
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

    private FeedAuditContext BuildAuditContext(string actorLogin, string actorDisplayName)
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
        return new FeedAuditContext(actorLogin, actorDisplayName, ipAddress, origin, userAgent);
    }
}
