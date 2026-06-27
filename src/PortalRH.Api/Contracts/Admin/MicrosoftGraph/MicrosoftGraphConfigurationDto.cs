namespace PortalRH.Api.Contracts.Admin.MicrosoftGraph;

public sealed record MicrosoftGraphConfigurationDto(
    Guid Id,
    bool IsEnabled,
    string TenantId,
    string ClientId,
    bool HasClientSecret,
    string UserIdentifier,
    DateTime UpdatedAtUtc);
