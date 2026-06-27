using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/agenda")]
[RequirePortalSession]
public class AgendaController : ControllerBase
{
    private readonly IAgendaService _agendaService;

    public AgendaController(IAgendaService agendaService)
    {
        _agendaService = agendaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AgendaDayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        var payload = await _agendaService.GetTodayAsync(session.PortalUserId, cancellationToken);
        return Ok(payload);
    }
}
