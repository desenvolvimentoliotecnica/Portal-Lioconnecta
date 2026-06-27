using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/mood-survey")]
[RequirePortalSession]
public class MoodSurveyController : ControllerBase
{
    private readonly IMoodSurveyService _moodSurveyService;

    public MoodSurveyController(IMoodSurveyService moodSurveyService)
    {
        _moodSurveyService = moodSurveyService;
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(MoodSurveyTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _moodSurveyService.GetTodayAsync(session.PortalUserId, cancellationToken);
        return Ok(payload);
    }

    [HttpPost("vote")]
    [ProducesResponseType(typeof(MoodSurveyTodayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitVote([FromBody] SubmitMoodSurveyVoteRequest request, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        try
        {
            var payload = await _moodSurveyService.SubmitVoteAsync(
                session.PortalUserId,
                request.OptionKey,
                BuildAuditContext(session.PortalUser.Login, session.PortalUser.DisplayName),
                cancellationToken);

            return Ok(payload);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(MoodSurveyDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.CanViewHrDashboard(session.PortalUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para consultar o dashboard de humor."
            });
        }

        var payload = await _moodSurveyService.GetDashboardAsync(startDate, endDate, department, cancellationToken);
        return Ok(payload);
    }

    private MoodSurveyAuditContext BuildAuditContext(string actorLogin, string actorDisplayName)
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
        return new MoodSurveyAuditContext(actorLogin, actorDisplayName, ipAddress, origin, userAgent);
    }
}
