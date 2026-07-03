using System.Collections.Concurrent;
using PortalLioConnecta.Api.Contracts.Journey;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class JourneyWorkspaceService : IJourneyWorkspaceService
{
    private const bool IsSimulated = true;
    private const string Provider = "ServiceNow";
    private const string DocumentsProvider = "GED";

    private static readonly ConcurrentDictionary<Guid, List<JourneyRequestItemDto>> CreatedRequestsByUser = new();
    private static readonly ConcurrentDictionary<Guid, List<JourneyTaskItemDto>> CreatedTasksByUser = new();

    private readonly IWebHostEnvironment _environment;
    private readonly IHrWorkspaceService _hrWorkspaceService;

    public JourneyWorkspaceService(IWebHostEnvironment environment, IHrWorkspaceService hrWorkspaceService)
    {
        _environment = environment;
        _hrWorkspaceService = hrWorkspaceService;
    }

    public Task<JourneyTasksResponse> GetTasksAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var items = BuildPendingTaskItems(user);
        var response = new JourneyTasksResponse(
            "Tarefas Pendentes",
            BuildTasksSummary(items),
            items,
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyCreateTaskResponse> CreateTaskAsync(
        PortalUser user,
        JourneyCreateTaskDto request,
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
        if (!JourneyTaskTypeCatalog.TryGet(typeKey, out var typeDefinition))
        {
            throw new InvalidOperationException("Tipo de tarefa invalido.");
        }

        var title = request.Title?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Titulo e obrigatorio.");
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

        ValidateTaskDueDate(request.DueDate);

        var item = new JourneyTaskItemDto(
            Guid.NewGuid(),
            typeDefinition.Key,
            typeDefinition.ListTypeLabel,
            title,
            priority,
            request.DueDate.Date,
            ResolveOpenStatus(request.DueDate.Date, typeDefinition.DefaultStatus),
            user.DisplayName,
            BuildTaskDescription(description, request.Fields),
            DateTime.UtcNow,
            true);

        var userTasks = CreatedTasksByUser.GetOrAdd(user.Id, _ => []);
        lock (userTasks)
        {
            userTasks.Insert(0, item);
        }

        var pendingItems = BuildPendingTaskItems(user);
        return Task.FromResult(new JourneyCreateTaskResponse(
            item,
            BuildTasksSummary(pendingItems),
            Provider,
            IsSimulated));
    }

    public Task<JourneyUpdateTaskResponse> UpdateTaskAsync(
        PortalUser user,
        Guid taskId,
        JourneyUpdateTaskDto request,
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

        var title = request.Title?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Titulo e obrigatorio.");
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

        ValidateTaskDueDate(request.DueDate);

        var userTasks = CreatedTasksByUser.GetOrAdd(user.Id, _ => []);
        JourneyTaskItemDto? updatedItem = null;

        lock (userTasks)
        {
            var index = userTasks.FindIndex(item => item.Id == taskId);
            if (index < 0)
            {
                throw new KeyNotFoundException("Tarefa nao encontrada ou nao editavel.");
            }

            var existing = userTasks[index];
            updatedItem = existing with
            {
                Title = title,
                Description = BuildTaskDescription(description, request.Fields),
                Priority = priority,
                DueDate = request.DueDate.Date,
                Status = ResolveOpenStatus(request.DueDate.Date, NormalizeOpenStatus(existing.Status))
            };
            userTasks[index] = updatedItem;
        }

        var pendingItems = BuildPendingTaskItems(user);
        return Task.FromResult(new JourneyUpdateTaskResponse(
            updatedItem!,
            BuildTasksSummary(pendingItems),
            Provider,
            IsSimulated));
    }

    public Task<JourneyUpdateTaskResponse> UpdateTaskStatusAsync(
        PortalUser user,
        Guid taskId,
        JourneyUpdateTaskStatusDto request,
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

        var status = request.Status?.Trim() ?? string.Empty;
        if (!IsAllowedStatusTransition(status))
        {
            throw new InvalidOperationException("Status invalido. Use Concluida ou Cancelada.");
        }

        var userTasks = CreatedTasksByUser.GetOrAdd(user.Id, _ => []);
        JourneyTaskItemDto? updatedItem = null;

        lock (userTasks)
        {
            var index = userTasks.FindIndex(item => item.Id == taskId);
            if (index < 0)
            {
                throw new KeyNotFoundException("Tarefa nao encontrada ou nao editavel.");
            }

            updatedItem = userTasks[index] with { Status = status };
            userTasks[index] = updatedItem;
        }

        var pendingItems = BuildPendingTaskItems(user);
        return Task.FromResult(new JourneyUpdateTaskResponse(
            updatedItem!,
            BuildTasksSummary(pendingItems),
            Provider,
            IsSimulated));
    }

    public async Task<JourneyRequestsResponse> GetRequestsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var items = BuildRequestItems(user).ToList();
        var hasRmItems = false;

        try
        {
            var vacation = await _hrWorkspaceService.GetVacationAsync(user, cancellationToken);
            if (vacation.AvailabilityStatus == "ok" && vacation.Requests.Count > 0)
            {
                var rmItems = vacation.Requests.Select(request => new JourneyRequestItemDto(
                    request.Id,
                    "Ferias (RM)",
                    $"{request.Days} dias • {request.StartDate:dd/MM/yyyy} a {request.EndDate:dd/MM/yyyy}",
                    request.RequestedAtUtc,
                    request.Status,
                    "TOTVS RM")).ToList();

                items = [.. rmItems, .. items.Where(item => item.Type != "Ferias")];
                hasRmItems = true;
            }
        }
        catch
        {
            // Mantem solicitacoes simuladas quando o RM nao estiver disponivel.
        }

        var response = new JourneyRequestsResponse(
            "Solicitacoes em Andamento",
            BuildRequestsSummary(items),
            items,
            hasRmItems ? "TOTVS RM" : Provider,
            !hasRmItems && IsSimulated);

        return response;
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

        var response = new JourneyDocumentsResponse(
            "Documentos Recentes",
            BuildDocumentItems(),
            DocumentsProvider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<JourneyDocumentContentDto?> GetDocumentContentAsync(
        PortalUser user,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var document = BuildDocumentItems().FirstOrDefault(item => item.Id == documentId);
        if (document is null)
        {
            return Task.FromResult<JourneyDocumentContentDto?>(null);
        }

        var filePath = Path.Combine(
            _environment.WebRootPath,
            "samples",
            "journey-documents",
            document.FileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<JourneyDocumentContentDto?>(null);
        }

        return Task.FromResult<JourneyDocumentContentDto?>(
            new JourneyDocumentContentDto(filePath, document.MimeType, document.FileName));
    }

    private static IReadOnlyList<JourneyDocumentItemDto> BuildDocumentItems()
    {
        var now = DateTime.UtcNow;

        return
        [
            new JourneyDocumentItemDto(
                Guid.Parse("b4000001-0000-4000-8000-000000000001"),
                "Politica de viagens corporativas v3.2",
                "Politicas",
                now.AddDays(-1),
                "1,8 MB",
                "Disponivel",
                "application/pdf",
                "politica-viagens.pdf"),
            new JourneyDocumentItemDto(
                Guid.Parse("b4000001-0000-4000-8000-000000000002"),
                "Manual do colaborador 2026",
                "Institucional",
                now.AddDays(-3),
                "4,2 MB",
                "Disponivel",
                "application/pdf",
                "manual-colaborador.pdf"),
            new JourneyDocumentItemDto(
                Guid.Parse("b4000001-0000-4000-8000-000000000003"),
                "Termo de confidencialidade assinado",
                "Contratos",
                now.AddDays(-7),
                "320 KB",
                "Assinado",
                "application/pdf",
                "termo-confidencialidade.pdf"),
            new JourneyDocumentItemDto(
                Guid.Parse("b4000001-0000-4000-8000-000000000004"),
                "Comprovante de treinamento NR-01",
                "Treinamentos",
                now.AddDays(-10),
                "980 KB",
                "Disponivel",
                "application/pdf",
                "comprovante-nr01.pdf")
        ];
    }

    private static IReadOnlyList<JourneyTaskItemDto> BuildPendingTaskItems(PortalUser user)
    {
        var today = DateTime.UtcNow.Date;
        var seedItems = BuildSeedTaskItems(user, today);
        var createdItems = CreatedTasksByUser.TryGetValue(user.Id, out var userTasks)
            ? userTasks.ToList()
            : [];

        var allItems = createdItems.Concat(seedItems).ToList();
        return allItems
            .Where(item => !IsClosedStatus(item.Status))
            .Select(item => item with { Status = ResolveOpenStatus(item.DueDate.Date, item.Status) })
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToList();
    }

    private static List<JourneyTaskItemDto> BuildSeedTaskItems(PortalUser user, DateTime today)
    {
        return
        [
            new JourneyTaskItemDto(
                Guid.Parse("b1000001-0000-4000-8000-000000000001"),
                "compliance-politica",
                "Politica / compliance",
                "Revisar politica de home office",
                "Alta",
                today.AddDays(1),
                "Em andamento",
                user.DisplayName,
                "Revisar e confirmar ciencia da politica de home office vigente.",
                today.AddDays(-4),
                false),
            new JourneyTaskItemDto(
                Guid.Parse("b1000001-0000-4000-8000-000000000002"),
                "rh-documento",
                "Documento RH",
                "Assinar termo de uso de equipamento",
                "Media",
                today,
                "Pendente",
                user.DisplayName,
                "Assinar termo de uso do equipamento corporativo.",
                today.AddDays(-2),
                false),
            new JourneyTaskItemDto(
                Guid.Parse("b1000001-0000-4000-8000-000000000003"),
                "rh-cadastro",
                "Cadastro RH",
                "Atualizar cadastro de dependentes",
                "Alta",
                today.AddDays(-1),
                "Atrasada",
                user.DisplayName,
                "Atualizar dependentes no cadastro funcional.",
                today.AddDays(-6),
                false),
            new JourneyTaskItemDto(
                Guid.Parse("b1000001-0000-4000-8000-000000000004"),
                "engajamento-pesquisa",
                "Pesquisa / enquete",
                "Responder pesquisa de clima",
                "Baixa",
                today.AddDays(3),
                "Pendente",
                user.DisplayName,
                "Responder pesquisa de clima organizacional 2026.",
                today.AddDays(-1),
                false),
            new JourneyTaskItemDto(
                Guid.Parse("b1000001-0000-4000-8000-000000000005"),
                "compliance-seguranca",
                "Seguranca da informacao",
                "Concluir onboarding de seguranca da informacao",
                "Media",
                today,
                "Em andamento",
                user.DisplayName,
                "Finalizar trilha obrigatoria de seguranca da informacao.",
                today.AddDays(-3),
                false)
        ];
    }

    private static JourneyTasksSummaryDto BuildTasksSummary(IReadOnlyList<JourneyTaskItemDto> items)
    {
        var today = DateTime.UtcNow.Date;
        var openItems = items.Where(item => !IsClosedStatus(item.Status)).ToList();

        var openCount = openItems.Count;
        var overdueCount = openItems.Count(item => item.DueDate.Date < today);
        var dueTodayCount = openItems.Count(item => item.DueDate.Date == today);

        return new JourneyTasksSummaryDto(openCount, overdueCount, dueTodayCount);
    }

    private static void ValidateTaskDueDate(DateTime dueDate)
    {
        if (dueDate.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("O prazo nao pode ser anterior a hoje.");
        }
    }

    private static bool IsAllowedStatusTransition(string status)
    {
        return status.Equals("Concluida", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsClosedStatus(string status)
    {
        return status.Equals("Concluida", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOpenStatus(string status)
    {
        return status.Equals("Pendente", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Em andamento", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Atrasada", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeOpenStatus(string status)
    {
        if (IsClosedStatus(status))
        {
            return "Pendente";
        }

        return IsOpenStatus(status) ? status : "Pendente";
    }

    private static string ResolveOpenStatus(DateTime dueDate, string status)
    {
        var normalized = NormalizeOpenStatus(status);
        if (dueDate.Date < DateTime.UtcNow.Date)
        {
            return "Atrasada";
        }

        return normalized;
    }

    private static string BuildTaskDescription(
        string description,
        IReadOnlyDictionary<string, string>? fields)
    {
        if (fields is null || fields.Count == 0)
        {
            return description;
        }

        var details = string.Join(
            " | ",
            fields
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
                .Select(entry => $"{entry.Key}: {entry.Value.Trim()}"));

        return string.IsNullOrWhiteSpace(details) ? description : $"{description} — {details}";
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
