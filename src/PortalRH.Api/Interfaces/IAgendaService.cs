using PortalRH.Api.Contracts.Agenda;

namespace PortalRH.Api.Interfaces;

public interface IAgendaService
{
    Task<AgendaDayResponse> GetTodayAsync(Guid portalUserId, CancellationToken cancellationToken);
}
