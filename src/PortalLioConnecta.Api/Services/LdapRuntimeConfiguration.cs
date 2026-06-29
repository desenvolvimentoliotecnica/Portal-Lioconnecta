namespace PortalLioConnecta.Api.Services;

public sealed record LdapRuntimeConfiguration(
    bool IsEnabled,
    string Server,
    int Port,
    bool UseLdaps,
    bool UseStartTls,
    bool IgnoreCertificateValidation,
    string BaseDn,
    string? UserSearchBase,
    string? NetbiosDomain,
    string LoginFormat,
    string? BindDn,
    string? BindPassword,
    string SearchFilter,
    string DisplayNameAttribute);
