namespace PortalLioConnecta.Api.Contracts.Admin.Auth;

public sealed record AdminLoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AdminProfileDto User);
