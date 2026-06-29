using PortalLioConnecta.Api.Contracts.Journey;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class JourneyService : IJourneyService
{
    public Task<JourneySummaryResponse> GetSummaryAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new JourneySummaryResponse(
            [
                new JourneyItemDto("Tarefas Pendentes", "5", "ServiceNow", "#minha-jornada/tarefas"),
                new JourneyItemDto("Solicitacoes em Andamento", "3", "ServiceNow", "#minha-jornada/solicitacoes"),
                new JourneyItemDto("Trilhas de Aprendizagem", "2", "LMS", "#minha-jornada/trilhas"),
                new JourneyItemDto("Documentos Recentes", "4", "GED", "#minha-jornada/documentos")
            ],
            "ServiceNow + LMS + GED",
            true);

        return Task.FromResult(response);
    }
}
