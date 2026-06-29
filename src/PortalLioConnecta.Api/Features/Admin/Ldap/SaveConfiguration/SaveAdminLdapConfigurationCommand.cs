using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Ldap;

namespace PortalLioConnecta.Api.Features.Admin.Ldap.SaveConfiguration;

public sealed record SaveAdminLdapConfigurationCommand(UpsertLdapConfigurationRequest Request) : IRequest<LdapConfigurationDto>;
