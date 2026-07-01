using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Ldap;

namespace PortalLioConnecta.Api.Features.Admin.Ldap.GetConfiguration;

public sealed record GetAdminLdapConfigurationQuery() : IRequest<LdapConfigurationDto>;
