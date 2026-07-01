namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record UpdatePortalUserModulePermissionRequest
{
    public string ModuleKey { get; init; } = string.Empty;
    public string AccessLevel { get; init; } = string.Empty;
}
