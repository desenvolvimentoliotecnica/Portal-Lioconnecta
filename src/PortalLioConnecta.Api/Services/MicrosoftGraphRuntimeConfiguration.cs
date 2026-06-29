namespace PortalLioConnecta.Api.Services;

public sealed record MicrosoftGraphRuntimeConfiguration(
    bool IsEnabled,
    string TenantId,
    string ClientId,
    string? ClientSecret,
    string UserIdentifier);
