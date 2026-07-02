namespace PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

public sealed record UpsertTotvsRmConfigurationRequest(
    bool IsEnabled,
    string Server,
    int Port,
    string Database,
    string UserName,
    string? Password,
    bool TrustServerCertificate);
