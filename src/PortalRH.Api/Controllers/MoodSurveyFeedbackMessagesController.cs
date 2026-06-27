using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/mood-survey/feedback-messages")]
[RequirePortalSession]
public class MoodSurveyFeedbackMessagesController : ControllerBase
{
    private readonly IMoodSurveyFeedbackService _feedbackService;

    public MoodSurveyFeedbackMessagesController(IMoodSurveyFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MoodSurveyFeedbackMessageListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] string? optionKey, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        var payload = await _feedbackService.GetAllAsync(optionKey, cancellationToken);
        return Ok(payload);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MoodSurveyFeedbackMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        var item = await _feedbackService.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MoodSurveyFeedbackMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        try
        {
            var item = await _feedbackService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MoodSurveyFeedbackMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        try
        {
            var item = await _feedbackService.UpdateAsync(id, request, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedActor(out var forbiddenResult))
        {
            return forbiddenResult!;
        }

        try
        {
            var deleted = await _feedbackService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private bool TryGetAuthorizedActor(out IActionResult? forbiddenResult)
    {
        forbiddenResult = null;
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            forbiddenResult = Unauthorized(new { message = "Sessao do portal nao encontrada." });
            return false;
        }

        if (!PortalModuleAccess.CanManageMoodSurveyFeedback(session.PortalUser))
        {
            forbiddenResult = StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para gerenciar mensagens de feedback do humor."
            });
            return false;
        }

        return true;
    }
}
