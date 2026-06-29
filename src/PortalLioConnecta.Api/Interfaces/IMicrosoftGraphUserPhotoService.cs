using PortalLioConnecta.Api.Contracts.Agenda;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IMicrosoftGraphUserPhotoService
{
    Task<string?> GetPhotoDataUrlForPortalUserAsync(PortalUser user, CancellationToken cancellationToken);

    Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> EnrichEventsWithParticipantPhotosAsync(
        string accessToken,
        IReadOnlyList<MicrosoftGraphCalendarEvent> events,
        CancellationToken cancellationToken);
}
