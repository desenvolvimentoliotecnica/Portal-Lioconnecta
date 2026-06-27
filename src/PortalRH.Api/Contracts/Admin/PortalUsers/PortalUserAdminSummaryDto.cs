namespace PortalRH.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserAdminSummaryDto(
    int RegisteredUsers,
    int ActiveUsers,
    int InactiveUsers,
    int DepartmentsMapped,
    int PortalAdmins,
    int LoginEvents,
    int FailedLoginEvents,
    int LogoutEvents,
    int MoodSurveyEvents);
