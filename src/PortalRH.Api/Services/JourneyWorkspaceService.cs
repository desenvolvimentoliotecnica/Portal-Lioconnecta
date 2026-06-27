using PortalRH.Api.Contracts.Journey;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class JourneyWorkspaceService : IJourneyWorkspaceService
{
    private const bool IsSimulated = true;

    public Task<JourneyTasksResponse> GetTasksAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var now = DateTime.UtcNow;
        var response = new JourneyTasksResponse(
            "Tarefas Pendentes",
            new JourneyTasksSummaryDto(5, 1, 2),
            [
                new JourneyTaskItemDto(
                    Guid.Parse("b1000001-0000-4000-8000-000000000001"),
                    "Revisar politica de home office",
                    "Alta",
                    now.Date.AddDays(1),
                    "Em andamento",
                    user.DisplayName),
                new JourneyTaskItemDto(
                    Guid.Parse("b1000001-0000-4000-8000-000000000002"),
                    "Assinar termo de uso de equipamento",
                    "Media",
                    now.Date,
                    "Pendente",
                    user.DisplayName),
                new JourneyTaskItemDto(
                    Guid.Parse("b1000001-0000-4000-8000-000000000003"),
                    "Atualizar cadastro de dependentes",
                    "Alta",
                    now.Date.AddDays(-1),
                    "Atrasada",
                    user.DisplayName),
                new JourneyTaskItemDto(
                    Guid.Parse("b1000001-0000-4000-8000-000000000004"),
                    "Responder pesquisa de clima",
                    "Baixa",
                    now.Date.AddDays(3),
                    "Pendente",
                    user.DisplayName),
                new JourneyTaskItemDto(
                    Guid.Parse("b1000001-0000-4000-8000-000000000005"),
                    "Concluir onboarding de seguranca da informacao",
                    "Media",
                    now.Date,
                    "Em andamento",
                    user.DisplayName)
            ],
            "ServiceNow",
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyRequestsResponse> GetRequestsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var now = DateTime.UtcNow;
        var response = new JourneyRequestsResponse(
            "Solicitacoes em Andamento",
            new JourneyRequestsSummaryDto(3, 1, 2),
            [
                new JourneyRequestItemDto(
                    Guid.Parse("b2000001-0000-4000-8000-000000000001"),
                    "Ferias",
                    "Solicitacao de 10 dias em julho/2026",
                    now.AddDays(-5),
                    "Em analise",
                    "Aprovacao do gestor"),
                new JourneyRequestItemDto(
                    Guid.Parse("b2000001-0000-4000-8000-000000000002"),
                    "Reembolso",
                    "Despesas de viagem corporativa - maio/2026",
                    now.AddDays(-2),
                    "Em andamento",
                    "Validacao financeira"),
                new JourneyRequestItemDto(
                    Guid.Parse("b2000001-0000-4000-8000-000000000003"),
                    "Equipamento",
                    "Troca de notebook por desgaste",
                    now.AddDays(-8),
                    "Aguardando",
                    "Triagem de TI")
            ],
            "ServiceNow",
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyLearningPathsResponse> GetLearningPathsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var now = DateTime.UtcNow;
        var response = new JourneyLearningPathsResponse(
            "Trilhas de Aprendizagem",
            new JourneyLearningPathsSummaryDto(2, 0, "6h30"),
            [
                new JourneyLearningPathItemDto(
                    Guid.Parse("b3000001-0000-4000-8000-000000000001"),
                    "Lideranca e feedback continuo",
                    65,
                    now.Date.AddDays(12),
                    "Em andamento",
                    "4h00"),
                new JourneyLearningPathItemDto(
                    Guid.Parse("b3000001-0000-4000-8000-000000000002"),
                    "Seguranca da informacao para colaboradores",
                    35,
                    now.Date.AddDays(20),
                    "Em andamento",
                    "2h30")
            ],
            "LMS",
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyDocumentsResponse> GetDocumentsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var now = DateTime.UtcNow;
        var response = new JourneyDocumentsResponse(
            "Documentos Recentes",
            [
                new JourneyDocumentItemDto(
                    Guid.Parse("b4000001-0000-4000-8000-000000000001"),
                    "Politica de viagens corporativas v3.2",
                    "Politicas",
                    now.AddDays(-1),
                    "1,8 MB",
                    "Disponivel"),
                new JourneyDocumentItemDto(
                    Guid.Parse("b4000001-0000-4000-8000-000000000002"),
                    "Manual do colaborador 2026",
                    "Institucional",
                    now.AddDays(-3),
                    "4,2 MB",
                    "Disponivel"),
                new JourneyDocumentItemDto(
                    Guid.Parse("b4000001-0000-4000-8000-000000000003"),
                    "Termo de confidencialidade assinado",
                    "Contratos",
                    now.AddDays(-7),
                    "320 KB",
                    "Assinado"),
                new JourneyDocumentItemDto(
                    Guid.Parse("b4000001-0000-4000-8000-000000000004"),
                    "Comprovante de treinamento NR-01",
                    "Treinamentos",
                    now.AddDays(-10),
                    "980 KB",
                    "Disponivel")
            ],
            "GED",
            IsSimulated);

        return Task.FromResult(response);
    }
}
