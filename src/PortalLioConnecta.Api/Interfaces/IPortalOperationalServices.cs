using PortalLioConnecta.Api.Contracts.CorporateSystems;
using PortalLioConnecta.Api.Contracts.HrProfile;
using PortalLioConnecta.Api.Contracts.Journey;
using PortalLioConnecta.Api.Contracts.Kpis;
using PortalLioConnecta.Api.Contracts.QuickLinks;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IQuickLinkService
{
    Task<QuickLinkListResponse> GetActiveAsync(CancellationToken cancellationToken);
    Task EnsureSeedAsync(CancellationToken cancellationToken);
}

public interface IJourneyService
{
    Task<JourneySummaryResponse> GetSummaryAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface IJourneyWorkspaceService
{
    Task<JourneyTasksResponse> GetTasksAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyCreateTaskResponse> CreateTaskAsync(PortalUser user, JourneyCreateTaskDto request, CancellationToken cancellationToken);
    Task<JourneyUpdateTaskResponse> UpdateTaskAsync(PortalUser user, Guid taskId, JourneyUpdateTaskDto request, CancellationToken cancellationToken);
    Task<JourneyUpdateTaskResponse> UpdateTaskStatusAsync(PortalUser user, Guid taskId, JourneyUpdateTaskStatusDto request, CancellationToken cancellationToken);
    Task<JourneyRequestsResponse> GetRequestsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyCreateRequestResponse> CreateRequestAsync(PortalUser user, JourneyCreateRequestDto request, CancellationToken cancellationToken);
    Task<JourneyLearningPathsResponse> GetLearningPathsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyLearningCatalogResponse> GetLearningCatalogAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyDocumentsResponse> GetDocumentsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyDocumentContentDto?> GetDocumentContentAsync(PortalUser user, Guid documentId, CancellationToken cancellationToken);
}

public interface IKpiService
{
    Task<KpiSummaryResponse> GetSummaryAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface IHrProfileService
{
    Task<HrProfileResponse> GetProfileAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface IHrWorkspaceService
{
    Task<HrVacationResponse> GetVacationAsync(PortalUser user, CancellationToken cancellationToken);
    Task<HrPayslipResponse> GetPayslipsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<HrPayslipDetailDto?> GetPayslipDetailAsync(PortalUser user, string payslipId, CancellationToken cancellationToken);
    Task<HrBenefitsResponse> GetBenefitsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<HrEvaluationResponse> GetEvaluationAsync(PortalUser user, CancellationToken cancellationToken);
    Task<HrPersonalDataResponse> GetPersonalDataAsync(PortalUser user, CancellationToken cancellationToken);
    Task<HrTimesheetResponse> GetTimesheetAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface ICorporateSystemsService
{
    Task<CorporateSystemsResponse> GetSystemsAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface IPortalPanelsComposer
{
    Task<Contracts.Shell.PanelsResponse> BuildAsync(PortalUser user, CancellationToken cancellationToken);
}

public interface IPortalUserSeedService
{
    Task EnsureSeedAsync(CancellationToken cancellationToken);
}
