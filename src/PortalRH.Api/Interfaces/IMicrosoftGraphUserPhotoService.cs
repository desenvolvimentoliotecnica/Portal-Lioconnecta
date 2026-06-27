using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Models;

namespace PortalRH.Api.Interfaces;

public interface IMicrosoftGraphUserPhotoService
{
    Task<string?> GetPhotoDataUrlForPortalUserAsync(PortalUser user, CancellationToken cancellationToken);

    Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> EnrichEventsWithParticipantPhotosAsync(
        string accessToken,
        IReadOnlyList<MicrosoftGraphCalendarEvent> events,
        CancellationToken cancellationToken);
}
