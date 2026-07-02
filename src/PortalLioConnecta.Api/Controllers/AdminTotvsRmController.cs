using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Features.Admin.TotvsRm.GetConfiguration;
using PortalLioConnecta.Api.Features.Admin.TotvsRm.SaveConfiguration;
using PortalLioConnecta.Api.Features.Admin.TotvsRm.TestConnection;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/admin/totvs-rm")]
[RequireSuperAdminSession]
public class AdminTotvsRmController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminTotvsRmController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TotvsRmConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminTotvsRmConfigurationQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(TotvsRmConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Save([FromBody] UpsertTotvsRmConfigurationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SaveAdminTotvsRmConfigurationCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(TotvsRmConnectionTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Test([FromBody] UpsertTotvsRmConfigurationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new TestAdminTotvsRmConnectionCommand(request), cancellationToken);
        return Ok(result);
    }
}
