using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Journey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/journey")]
[RequirePortalSession]
public class JourneyController : ControllerBase
{
    private readonly IJourneyService _journeyService;
    private readonly IJourneyWorkspaceService _journeyWorkspaceService;

    public JourneyController(
        IJourneyService journeyService,
        IJourneyWorkspaceService journeyWorkspaceService)
    {
        _journeyService = journeyService;
        _journeyWorkspaceService = journeyWorkspaceService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(JourneySummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        => await ExecuteAsync(user => _journeyService.GetSummaryAsync(user, cancellationToken));

    [HttpGet("tarefas")]
    [ProducesResponseType(typeof(JourneyTasksResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTasks(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetTasksAsync(user, cancellationToken));

    [HttpGet("solicitacoes")]
    [ProducesResponseType(typeof(JourneyRequestsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetRequests(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetRequestsAsync(user, cancellationToken));

    [HttpGet("trilhas")]
    [ProducesResponseType(typeof(JourneyLearningPathsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetLearningPaths(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetLearningPathsAsync(user, cancellationToken));

    [HttpGet("documentos")]
    [ProducesResponseType(typeof(JourneyDocumentsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDocuments(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetDocumentsAsync(user, cancellationToken));

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
