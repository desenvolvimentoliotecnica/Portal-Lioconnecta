using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/admin/mood-survey")]
[RequireSuperAdminSession]
public class AdminMoodSurveyController : ControllerBase
{
    private readonly IMoodSurveyService _moodSurveyService;

    public AdminMoodSurveyController(IMoodSurveyService moodSurveyService)
    {
        _moodSurveyService = moodSurveyService;
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
        var payload = await _moodSurveyService.GetDashboardAsync(startDate, endDate, department, cancellationToken);
        return Ok(payload);
    }
}
