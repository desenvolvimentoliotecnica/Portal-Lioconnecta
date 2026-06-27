using PortalRH.Api.Contracts.Journey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

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
