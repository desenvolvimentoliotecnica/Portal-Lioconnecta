namespace PortalLioConnecta.Api.Contracts.Admin.Auth;

public sealed record AdminSessionDto(
    string Token,
    DateTime ExpiresAtUtc,
    AdminProfileDto User);
