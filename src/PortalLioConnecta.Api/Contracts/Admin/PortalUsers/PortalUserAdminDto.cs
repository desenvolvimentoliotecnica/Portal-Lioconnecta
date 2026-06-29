namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserAdminDto(
    Guid Id,
    string Login,
    string DisplayName,
    string? Email,
    string? Department,
    string? Title,
    string? ManagerDisplayName,
    string AuthenticationProvider,
    string Role,
    string RoleLabel,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<PortalUserModulePermissionDto> ModulePermissions,
    bool IsActive,
    int LoginCount,
    int FailedLoginCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? LastLoginAtUtc,
    DateTime? LastFailedLoginAtUtc,
    string? LastKnownIpAddress,
    string? LastOrigin);
