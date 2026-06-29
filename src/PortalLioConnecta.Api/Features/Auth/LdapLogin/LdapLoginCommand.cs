using MediatR;
using PortalLioConnecta.Api.Contracts.Auth;

namespace PortalLioConnecta.Api.Features.Auth.LdapLogin;

public sealed record LdapLoginCommand(LdapLoginRequest Request) : IRequest<PortalLoginResponse?>;
