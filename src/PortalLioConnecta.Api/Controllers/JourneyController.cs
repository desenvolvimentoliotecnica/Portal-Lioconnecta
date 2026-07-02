using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Journey;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

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

    [HttpPost("solicitacoes")]
    [ProducesResponseType(typeof(JourneyCreateRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest(
        [FromBody] JourneyCreateRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var payload = await _journeyWorkspaceService.CreateRequestAsync(session.PortalUser, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, payload);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("trilhas")]
    [ProducesResponseType(typeof(JourneyLearningPathsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetLearningPaths(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetLearningPathsAsync(user, cancellationToken));

    [HttpGet("trilhas/cursos-materiais")]
    [ProducesResponseType(typeof(JourneyLearningCatalogResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetLearningCatalog(CancellationToken cancellationToken)
        => ExecuteAsync(user => _journeyWorkspaceService.GetLearningCatalogAsync(user, cancellationToken));

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
