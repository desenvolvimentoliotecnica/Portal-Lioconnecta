namespace PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

public sealed record UpsertTotvsRmConfigurationRequest(
    bool IsEnabled,
    string Server,
    int Port,
    string Database,
    string UserName,
    string? Password,
    bool TrustServerCertificate,
    short CodColigada,
    bool EnableCadastro,
    bool EnableHolerite,
    bool EnableFerias,
    bool EnableBeneficios,
    bool EnablePonto,
    bool EnableTeamDashboard);
