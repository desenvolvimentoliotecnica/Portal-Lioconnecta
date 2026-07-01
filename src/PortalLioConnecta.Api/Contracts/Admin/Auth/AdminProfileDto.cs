namespace PortalLioConnecta.Api.Contracts.Admin.Auth;

public sealed record AdminProfileDto(
    Guid Id,
    string Username,
    string DisplayName,
    string Role);
