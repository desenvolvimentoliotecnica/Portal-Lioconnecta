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
    short CodColigada,
    bool EnableCadastro,
    bool EnableHolerite,
    bool EnableFerias,
    bool EnableBeneficios,
    bool EnablePonto,
    bool EnableTeamDashboard,
    DateTime UpdatedAtUtc);
