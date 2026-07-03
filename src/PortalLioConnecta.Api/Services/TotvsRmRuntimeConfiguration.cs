namespace PortalLioConnecta.Api.Services;

public sealed record TotvsRmRuntimeConfiguration(
    bool IsEnabled,
    string Server,
    int Port,
    string Database,
    string UserName,
    string? Password,
    bool TrustServerCertificate,
    short CodColigada,
    TotvsRmModuleFlags ModuleFlags,
    int TimesheetPeriodStartDay,
    int TimesheetPeriodEndDay);

public sealed record TotvsRmModuleFlags(
    bool Cadastro,
    bool Holerite,
    bool Ferias,
    bool Beneficios,
    bool Ponto,
    bool TeamDashboard)
{
    public static TotvsRmModuleFlags AllEnabled { get; } = new(true, true, true, true, true, true);

    public bool HasAnyEnabled =>
        Cadastro || Holerite || Ferias || Beneficios || Ponto || TeamDashboard;
}
