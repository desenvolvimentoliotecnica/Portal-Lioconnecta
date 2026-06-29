using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Admin.Ldap;

public class UpsertLdapConfigurationRequest
{
    public bool IsEnabled { get; set; }

    [Required]
    [MaxLength(200)]
    public string Server { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 389;

    public bool UseLdaps { get; set; }
    public bool UseStartTls { get; set; }
    public bool IgnoreCertificateValidation { get; set; }

    [Required]
    [MaxLength(240)]
    public string BaseDn { get; set; } = string.Empty;

    [MaxLength(240)]
    public string? UserSearchBase { get; set; }

    [MaxLength(120)]
    public string? NetbiosDomain { get; set; }

    [Required]
    [MaxLength(80)]
    public string LoginFormat { get; set; } = string.Empty;

    [MaxLength(240)]
    public string? BindDn { get; set; }

    public string? ServiceAccountPassword { get; set; }

    [Required]
    [MaxLength(500)]
    public string SearchFilter { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string DisplayNameAttribute { get; set; } = string.Empty;
}
