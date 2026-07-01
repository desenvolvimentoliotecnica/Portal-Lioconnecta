using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Admin.Auth;
using PortalLioConnecta.Api.Features.Admin.Auth.GetSession;
using PortalLioConnecta.Api.Features.Admin.Auth.Login;
using PortalLioConnecta.Api.Features.Admin.Auth.Logout;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var session = await _mediator.Send(new LoginAdminCommand(request), cancellationToken);
        return session is null
            ? Unauthorized(new { message = "Usuario ou senha invalidos." })
            : Ok(session);
    }

    [HttpGet("session")]
    [RequireAdminSession]
    [ProducesResponseType(typeof(AdminSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSession(CancellationToken cancellationToken)
    {
        var token = ReadToken();
        var session = await _mediator.Send(new GetAdminSessionQuery(token), cancellationToken);
        return session is null
            ? Unauthorized(new { message = "Sessao administrativa invalida ou expirada." })
            : Ok(session);
    }

    [HttpPost("logout")]
    [RequireAdminSession]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = ReadToken();
        await _mediator.Send(new LogoutAdminCommand(token), cancellationToken);
        return NoContent();
    }

    private string ReadToken()
    {
        if (Request.Headers.TryGetValue("X-Admin-Token", out var adminToken))
        {
            return adminToken.ToString();
        }

        var authorization = Request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";
        return authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization[bearerPrefix.Length..].Trim()
            : string.Empty;
    }
}
