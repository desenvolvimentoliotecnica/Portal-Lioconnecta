using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/hr")]
[RequirePortalSession]
public class HrWorkspaceController : ControllerBase
{
    private readonly IHrWorkspaceService _hrWorkspaceService;

    public HrWorkspaceController(IHrWorkspaceService hrWorkspaceService)
    {
        _hrWorkspaceService = hrWorkspaceService;
    }

    [HttpGet("ferias")]
    [ProducesResponseType(typeof(HrVacationResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetVacation(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetVacationAsync(user, cancellationToken));

    [HttpGet("holerite")]
    [ProducesResponseType(typeof(HrPayslipResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPayslips(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetPayslipsAsync(user, cancellationToken));

    [HttpGet("holerite/{id}")]
    [ProducesResponseType(typeof(HrPayslipDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayslipDetail(string id, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _hrWorkspaceService.GetPayslipDetailAsync(session.PortalUser, id, cancellationToken);
        return payload is null ? NotFound(new { message = "Holerite nao encontrado." }) : Ok(payload);
    }

    [HttpGet("beneficios")]
    [ProducesResponseType(typeof(HrBenefitsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetBenefits(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetBenefitsAsync(user, cancellationToken));

    [HttpGet("avaliacao")]
    [ProducesResponseType(typeof(HrEvaluationResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetEvaluation(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetEvaluationAsync(user, cancellationToken));

    [HttpGet("cadastro")]
    [ProducesResponseType(typeof(HrPersonalDataResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPersonalData(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetPersonalDataAsync(user, cancellationToken));

    [HttpGet("ponto")]
    [ProducesResponseType(typeof(HrTimesheetResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTimesheet(CancellationToken cancellationToken)
        => ExecuteAsync(user => _hrWorkspaceService.GetTimesheetAsync(user, cancellationToken));

    private async Task<IActionResult> ExecuteAsync<TResponse>(
        Func<PortalUser, Task<TResponse>> action)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await action(session.PortalUser);
        return Ok(payload);
    }
}
