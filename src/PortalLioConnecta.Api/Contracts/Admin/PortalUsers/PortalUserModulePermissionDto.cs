namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserModulePermissionDto(
    string ModuleKey,
    string ModuleLabel,
    string AccessLevel,
    string AccessLevelLabel);
