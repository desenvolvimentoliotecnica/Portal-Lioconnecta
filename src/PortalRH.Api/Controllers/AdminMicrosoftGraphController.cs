using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Features.Admin.MicrosoftGraph.GetConfiguration;
using PortalRH.Api.Features.Admin.MicrosoftGraph.SaveConfiguration;
using PortalRH.Api.Features.Admin.MicrosoftGraph.TestConnection;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/admin/microsoft-graph")]
[RequireAdminSession]
public class AdminMicrosoftGraphController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminMicrosoftGraphController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MicrosoftGraphConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminMicrosoftGraphConfigurationQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(MicrosoftGraphConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Save([FromBody] UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SaveAdminMicrosoftGraphConfigurationCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(MicrosoftGraphConnectionTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Test([FromBody] UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new TestAdminMicrosoftGraphConnectionCommand(request), cancellationToken);
        return Ok(result);
    }
}
