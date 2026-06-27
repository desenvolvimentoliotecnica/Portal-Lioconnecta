using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Notifications;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[RequirePortalSession]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _notificationService.GetForUserAsync(session.PortalUserId, cancellationToken);
        return Ok(payload);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var marked = await _notificationService.MarkAsReadAsync(id, session.PortalUserId, cancellationToken);
        return marked ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var count = await _notificationService.MarkAllAsReadAsync(session.PortalUserId, cancellationToken);
        return Ok(new { markedCount = count });
    }
}
