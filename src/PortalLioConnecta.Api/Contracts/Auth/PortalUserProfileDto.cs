namespace PortalLioConnecta.Api.Contracts.Auth;

public sealed record PortalUserProfileDto(
    Guid Id,
    string Login,
    string DisplayName,
    string? Email,
    string? Department,
    string? Title,
    string? ManagerDisplayName,
    string Role,
    string RoleLabel,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<PortalLioConnecta.Api.Contracts.Admin.PortalUsers.PortalUserModulePermissionDto> ModulePermissions);
