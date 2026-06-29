namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserRoleOptionDto(
    string Key,
    string Label,
    IReadOnlyList<string> Permissions);
