using PortalRH.Api.Contracts.Kpis;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class KpiService : IKpiService
{
    public Task<KpiSummaryResponse> GetSummaryAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new KpiSummaryResponse(
            [
                new KpiItemDto("Presenca Hoje", "92%", "BI Corporativo"),
                new KpiItemDto("Chamados Abertos", "14", "ServiceNow"),
                new KpiItemDto("Projetos Ativos", "7", "PMO"),
                new KpiItemDto("Eventos da Semana", "5", "Agenda Corporativa"),
                new KpiItemDto("Treinamentos do Mes", "3", "LMS")
            ],
            "BI + ServiceNow",
            true);

        return Task.FromResult(response);
    }
}
