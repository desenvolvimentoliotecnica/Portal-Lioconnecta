using PortalRH.Api.Contracts.CorporateSystems;
using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Contracts.Journey;
using PortalRH.Api.Contracts.Kpis;
using PortalRH.Api.Contracts.QuickLinks;
using PortalRH.Api.Models;

namespace PortalRH.Api.Interfaces;

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
    Task<JourneyRequestsResponse> GetRequestsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyLearningPathsResponse> GetLearningPathsAsync(PortalUser user, CancellationToken cancellationToken);
    Task<JourneyDocumentsResponse> GetDocumentsAsync(PortalUser user, CancellationToken cancellationToken);
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
