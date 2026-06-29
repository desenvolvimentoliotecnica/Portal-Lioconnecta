namespace PortalLioConnecta.Api.Contracts.Admin.Auth;

public sealed record AdminLoginRequest(
    string Username,
    string Password);
