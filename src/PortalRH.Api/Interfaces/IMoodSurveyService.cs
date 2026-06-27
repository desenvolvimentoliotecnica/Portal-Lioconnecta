using PortalRH.Api.Contracts.MoodSurvey;

namespace PortalRH.Api.Interfaces;

public interface IMoodSurveyService
{
    Task<MoodSurveyTodayResponse> GetTodayAsync(Guid portalUserId, CancellationToken cancellationToken);

    Task<MoodSurveyTodayResponse> SubmitVoteAsync(
        Guid portalUserId,
        string optionKey,
        MoodSurveyAuditContext auditContext,
        CancellationToken cancellationToken);

    Task<MoodSurveyDashboardResponse> GetDashboardAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? department,
        CancellationToken cancellationToken);
}

public sealed record MoodSurveyAuditContext(
    string ActorLogin,
    string ActorDisplayName,
    string? IpAddress,
    string? Origin,
    string? UserAgent);
