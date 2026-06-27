using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Admin.Auth;
using PortalRH.Api.Contracts.Admin.Polls;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/admin/polls")]
[RequirePortalSession]
public class AdminPollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public AdminPollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PollAdminListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var actor, out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        var items = await _pollService.GetAdminListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PollAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out _, out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        var item = await _pollService.GetAdminByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PollAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UpsertPollRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var actor, out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        try
        {
            var item = await _pollService.CreateAsync(request, actor!, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PollAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPollRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var actor, out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        try
        {
            var item = await _pollService.UpdateAsync(id, request, actor!, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(PollAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePollStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var actor, out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        var item = await _pollService.UpdateStatusAsync(id, request.Status, actor!, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    private bool TryGetAuthorizedActor(out AdminProfileDto? actor, out IActionResult? forbiddenResult)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        var user = session?.PortalUser;
        if (user is null)
        {
            actor = null;
            forbiddenResult = Unauthorized(new { message = "Sessao do portal nao encontrada." });
            return false;
        }

        if (!PortalModuleAccess.CanManagePolls(user))
        {
            actor = null;
            forbiddenResult = StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para gerenciar enquetes."
            });
            return false;
        }

        actor = new AdminProfileDto(user.Id, user.Login, user.DisplayName, user.Role);
        forbiddenResult = null;
        return true;
    }
}
