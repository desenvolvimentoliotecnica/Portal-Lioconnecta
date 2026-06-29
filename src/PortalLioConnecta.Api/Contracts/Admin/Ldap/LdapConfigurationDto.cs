namespace PortalLioConnecta.Api.Contracts.Admin.Ldap;

public sealed record LdapConfigurationDto(
    Guid Id,
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
    bool HasServiceAccountPassword,
    string SearchFilter,
    string DisplayNameAttribute,
    DateTime UpdatedAtUtc);
