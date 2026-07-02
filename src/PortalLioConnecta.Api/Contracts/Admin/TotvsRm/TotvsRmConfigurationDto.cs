namespace PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

public sealed record TotvsRmConfigurationDto(
    Guid Id,
    bool IsEnabled,
    string Server,
    int Port,
    string Database,
    string UserName,
    bool HasPassword,
    bool TrustServerCertificate,
    DateTime UpdatedAtUtc);
