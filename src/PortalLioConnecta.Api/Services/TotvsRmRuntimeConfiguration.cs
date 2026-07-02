namespace PortalLioConnecta.Api.Services;

public sealed record TotvsRmRuntimeConfiguration(
    bool IsEnabled,
    string Server,
    int Port,
    string Database,
    string UserName,
    string? Password,
    bool TrustServerCertificate);
