using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Auth;
using PortalLioConnecta.Api.Features.Auth.LdapLogin;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPortalAuthService _portalAuthService;

    public AuthController(IMediator mediator, IPortalAuthService portalAuthService)
    {
        _mediator = mediator;
        _portalAuthService = portalAuthService;
    }

    [HttpPost("ldap/login")]
    [ProducesResponseType(typeof(PortalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> LoginWithLdap([FromBody] LdapLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LdapLoginCommand(request), cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Nao foi possivel autenticar com o LDAP. Revise as credenciais ou a configuracao ativa."
            });
        }

        return Ok(result);
    }

    [HttpGet("session")]
    [ProducesResponseType(typeof(PortalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSession(CancellationToken cancellationToken)
    {
        var token = ResolvePortalToken();
        var session = await _portalAuthService.GetSessionAsync(token, cancellationToken);

        return session is null
            ? Unauthorized(new { message = "Sessao do portal invalida ou expirada." })
            : Ok(session);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = ResolvePortalToken();
        await _portalAuthService.LogoutAsync(token, cancellationToken);
        return NoContent();
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
