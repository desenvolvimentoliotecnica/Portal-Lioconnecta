using PortalRH.Api.Contracts.MoodSurvey;

namespace PortalRH.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserAdminListResponse(
    IReadOnlyList<PortalUserAdminDto> Items,
    PortalUserAdminSummaryDto Summary,
    IReadOnlyList<PortalUserRoleOptionDto> RoleOptions,
    IReadOnlyList<PortalUserDepartmentOptionDto> DepartmentOptions,
    IReadOnlyList<PortalUserModuleOptionDto> ModuleOptions,
    IReadOnlyList<PortalUserAccessLevelOptionDto> AccessLevelOptions,
    IReadOnlyList<PortalUserLoginEventDto> RecentLogins,
    IReadOnlyList<PortalUserAdminAuditLogDto> RecentAuditEntries,
    IReadOnlyList<MoodSurveyAuditLogDto> RecentMoodSurveyEntries,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    string Query,
    string Status,
    string Role,
    string Department,
    string SortBy,
    string SortDirection);
