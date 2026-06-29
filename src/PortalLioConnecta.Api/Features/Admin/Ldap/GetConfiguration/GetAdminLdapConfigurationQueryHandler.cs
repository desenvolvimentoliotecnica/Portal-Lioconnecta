using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Ldap;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.Ldap.GetConfiguration;

public class GetAdminLdapConfigurationQueryHandler : IRequestHandler<GetAdminLdapConfigurationQuery, LdapConfigurationDto>
{
    private readonly ILdapConfigurationService _ldapConfigurationService;

    public GetAdminLdapConfigurationQueryHandler(ILdapConfigurationService ldapConfigurationService)
    {
        _ldapConfigurationService = ldapConfigurationService;
    }

    public Task<LdapConfigurationDto> Handle(GetAdminLdapConfigurationQuery request, CancellationToken cancellationToken)
    {
        return _ldapConfigurationService.GetAsync(cancellationToken);
    }
}
