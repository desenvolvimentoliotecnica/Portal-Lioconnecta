namespace PortalLioConnecta.Api.Services;

public sealed record LdapAuthenticatedUser(
    string Login,
    string? SamAccountName,
    string? UserPrincipalName,
    string? Email,
    string DisplayName,
    string? Department,
    string? Title,
    string? DistinguishedName,
    string? ManagerDisplayName,
    string? ManagerDistinguishedName);
