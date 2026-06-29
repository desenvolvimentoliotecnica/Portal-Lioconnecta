using PortalLioConnecta.Api.Contracts.Admin.Ldap;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Interfaces;

public interface ILdapConfigurationService
{
    Task<LdapConfigurationDto> GetAsync(CancellationToken cancellationToken);
    Task<LdapConfigurationDto> SaveAsync(UpsertLdapConfigurationRequest request, CancellationToken cancellationToken);
    Task<LdapRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken);
    Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken);
}
