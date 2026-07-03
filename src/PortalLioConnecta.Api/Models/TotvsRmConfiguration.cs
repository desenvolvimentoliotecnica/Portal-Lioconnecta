namespace PortalLioConnecta.Api.Models;

public class TotvsRmConfiguration
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 1433;
    public string Database { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PasswordProtected { get; set; }
    public bool TrustServerCertificate { get; set; } = true;
    public short CodColigada { get; set; } = 1;
    public bool EnableCadastro { get; set; } = true;
    public bool EnableHolerite { get; set; } = true;
    public bool EnableFerias { get; set; } = true;
    public bool EnableBeneficios { get; set; } = true;
    public bool EnablePonto { get; set; } = true;
    public bool EnableTeamDashboard { get; set; } = true;
    public int TimesheetPeriodStartDay { get; set; } = 16;
    public int TimesheetPeriodEndDay { get; set; } = 15;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
