using PortalLioConnecta.Api.Contracts.Agenda;

namespace PortalLioConnecta.Api.Interfaces;

public interface IAgendaService
{
    Task<AgendaDayResponse> GetTodayAsync(Guid portalUserId, CancellationToken cancellationToken);
}
