using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Ldap;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.Ldap.SaveConfiguration;

public class SaveAdminLdapConfigurationCommandHandler : IRequestHandler<SaveAdminLdapConfigurationCommand, LdapConfigurationDto>
{
    private readonly ILdapConfigurationService _ldapConfigurationService;

    public SaveAdminLdapConfigurationCommandHandler(ILdapConfigurationService ldapConfigurationService)
    {
        _ldapConfigurationService = ldapConfigurationService;
    }

    public Task<LdapConfigurationDto> Handle(SaveAdminLdapConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _ldapConfigurationService.SaveAsync(request.Request, cancellationToken);
    }
}
