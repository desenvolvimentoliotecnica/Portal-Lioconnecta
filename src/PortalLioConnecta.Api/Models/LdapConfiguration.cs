namespace PortalLioConnecta.Api.Models;

public class LdapConfiguration
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 389;
    public bool UseLdaps { get; set; }
    public bool UseStartTls { get; set; }
    public bool IgnoreCertificateValidation { get; set; }
    public string BaseDn { get; set; } = string.Empty;
    public string? UserSearchBase { get; set; }
    public string? NetbiosDomain { get; set; }
    public string LoginFormat { get; set; } = string.Empty;
    public string? BindDn { get; set; }
    public string? BindPasswordProtected { get; set; }
    public string SearchFilter { get; set; } = string.Empty;
    public string DisplayNameAttribute { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
