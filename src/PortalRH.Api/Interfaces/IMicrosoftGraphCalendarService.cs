using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Models;

namespace PortalRH.Api.Interfaces;

public interface IMicrosoftGraphCalendarService
{
    Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> GetUpcomingEventsAsync(
        PortalUser user,
        int limit,
        CancellationToken cancellationToken);
}

public sealed record MicrosoftGraphCalendarEvent(
    string Id,
    string Title,
    string? Description,
    string? Location,
    string? JoinUrl,
    IReadOnlyList<AgendaParticipantDto> Participants,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool IsAllDay);
