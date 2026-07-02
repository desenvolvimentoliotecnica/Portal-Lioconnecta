using System.Collections.Concurrent;
using PortalLioConnecta.Api.Contracts.Journey;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class JourneyWorkspaceService : IJourneyWorkspaceService
{
    private const bool IsSimulated = true;
    private const string Provider = "ServiceNow";

    private static readonly ConcurrentDictionary<Guid, List<JourneyRequestItemDto>> CreatedRequestsByUser = new();

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
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyRequestsResponse> GetRequestsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var items = BuildRequestItems(user);
        var response = new JourneyRequestsResponse(
            "Solicitacoes em Andamento",
            BuildRequestsSummary(items),
            items,
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyCreateRequestResponse> CreateRequestAsync(
        PortalUser user,
        JourneyCreateRequestDto request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var typeKey = request.TypeKey?.Trim() ?? string.Empty;
        if (!JourneyRequestTypeCatalog.TryGet(typeKey, out var typeDefinition))
        {
            throw new InvalidOperationException("Tipo de solicitacao invalido.");
        }

        var subject = request.Subject?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Assunto e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Descricao e obrigatoria.");
        }

        var priority = request.Priority?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(priority))
        {
            throw new InvalidOperationException("Prioridade e obrigatoria.");
        }

        var item = new JourneyRequestItemDto(
            Guid.NewGuid(),
            typeDefinition.ListTypeLabel,
            BuildRequestDescription(subject, description, request.Fields),
            DateTime.UtcNow,
            typeDefinition.DefaultStatus,
            typeDefinition.DefaultStage);

        var userRequests = CreatedRequestsByUser.GetOrAdd(user.Id, _ => []);
        lock (userRequests)
        {
            userRequests.Insert(0, item);
        }

        var allItems = BuildRequestItems(user);
        var response = new JourneyCreateRequestResponse(
            item,
            BuildRequestsSummary(allItems),
            Provider,
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

    public Task<JourneyLearningCatalogResponse> GetLearningCatalogAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var courses = new[]
        {
            new JourneyLearningCourseDto(
                Guid.Parse("b3100001-0000-4000-8000-000000000001"),
                "Fundamentos de lideranca situacional",
                "Conceitos essenciais para conduzir equipes com feedback continuo e metas claras.",
                "1h30",
                "Videoaula",
                "Em andamento",
                "Lideranca e feedback continuo"),
            new JourneyLearningCourseDto(
                Guid.Parse("b3100001-0000-4000-8000-000000000002"),
                "Feedback continuo na pratica",
                "Tecnicas para conversas de desenvolvimento e acompanhamento de desempenho.",
                "1h00",
                "Curso online",
                "Disponivel",
                "Lideranca e feedback continuo"),
            new JourneyLearningCourseDto(
                Guid.Parse("b3100001-0000-4000-8000-000000000003"),
                "Boas praticas de seguranca da informacao",
                "Protecao de dados, senhas fortes e prevencao de phishing no dia a dia.",
                "1h15",
                "Videoaula",
                "Em andamento",
                "Seguranca da informacao para colaboradores"),
            new JourneyLearningCourseDto(
                Guid.Parse("b3100001-0000-4000-8000-000000000004"),
                "LGPD para colaboradores",
                "Obrigacoes legais e cuidados no tratamento de dados pessoais.",
                "45min",
                "Curso online",
                "Disponivel",
                "Seguranca da informacao para colaboradores"),
            new JourneyLearningCourseDto(
                Guid.Parse("b3100001-0000-4000-8000-000000000005"),
                "Comunicacao assertiva para lideres",
                "Como alinhar expectativas e conduzir reunioes produtivas.",
                "1h15",
                "Videoaula",
                "Disponivel",
                "Lideranca e feedback continuo")
        };

        var materials = new[]
        {
            new JourneyLearningMaterialDto(
                Guid.Parse("b3200001-0000-4000-8000-000000000001"),
                "Guia de feedback continuo",
                "PDF",
                "2,4 MB",
                "Disponivel"),
            new JourneyLearningMaterialDto(
                Guid.Parse("b3200001-0000-4000-8000-000000000002"),
                "Checklist de reuniao 1:1",
                "Planilha",
                "180 KB",
                "Disponivel"),
            new JourneyLearningMaterialDto(
                Guid.Parse("b3200001-0000-4000-8000-000000000003"),
                "Politica de seguranca da informacao",
                "PDF",
                "1,1 MB",
                "Disponivel"),
            new JourneyLearningMaterialDto(
                Guid.Parse("b3200001-0000-4000-8000-000000000004"),
                "Slides - Prevencao de phishing",
                "Apresentacao",
                "3,6 MB",
                "Disponivel"),
            new JourneyLearningMaterialDto(
                Guid.Parse("b3200001-0000-4000-8000-000000000005"),
                "Modelo de plano de desenvolvimento individual",
                "Planilha",
                "240 KB",
                "Disponivel")
        };

        var response = new JourneyLearningCatalogResponse(
            "Cursos e Materiais disponiveis",
            new JourneyLearningCatalogSummaryDto(courses.Length, materials.Length, "6h45"),
            courses,
            materials,
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

    private static IReadOnlyList<JourneyRequestItemDto> BuildRequestItems(PortalUser user)
    {
        var now = DateTime.UtcNow;
        var seedItems = new List<JourneyRequestItemDto>
        {
            new(
                Guid.Parse("b2000001-0000-4000-8000-000000000001"),
                "Ferias",
                "Solicitacao de 10 dias em julho/2026",
                now.AddDays(-5),
                "Em analise",
                "Aprovacao do gestor"),
            new(
                Guid.Parse("b2000001-0000-4000-8000-000000000002"),
                "Reembolso",
                "Despesas de viagem corporativa - maio/2026",
                now.AddDays(-2),
                "Em andamento",
                "Validacao financeira"),
            new(
                Guid.Parse("b2000001-0000-4000-8000-000000000003"),
                "Equipamento",
                "Troca de notebook por desgaste",
                now.AddDays(-8),
                "Aguardando",
                "Triagem de TI")
        };

        if (!CreatedRequestsByUser.TryGetValue(user.Id, out var createdItems) || createdItems.Count == 0)
        {
            return seedItems;
        }

        lock (createdItems)
        {
            return [.. createdItems, .. seedItems];
        }
    }

    private static JourneyRequestsSummaryDto BuildRequestsSummary(IReadOnlyList<JourneyRequestItemDto> items)
    {
        var pendingApprovalCount = items.Count(item =>
        {
            var status = item.Status.ToLowerInvariant();
            return status.Contains("analise", StringComparison.Ordinal) || status.Contains("aguard", StringComparison.Ordinal);
        });

        var inProgressCount = items.Count(item =>
        {
            var status = item.Status.ToLowerInvariant();
            return status.Contains("andamento", StringComparison.Ordinal) || status.Contains("process", StringComparison.Ordinal);
        });

        return new JourneyRequestsSummaryDto(items.Count, pendingApprovalCount, inProgressCount);
    }

    private static string BuildRequestDescription(
        string subject,
        string description,
        IReadOnlyDictionary<string, string>? fields)
    {
        if (fields is null || fields.Count == 0)
        {
            return subject;
        }

        var details = string.Join(
            " | ",
            fields
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
                .Select(entry => $"{entry.Key}: {entry.Value.Trim()}"));

        return string.IsNullOrWhiteSpace(details) ? subject : $"{subject} — {details}";
    }
}
