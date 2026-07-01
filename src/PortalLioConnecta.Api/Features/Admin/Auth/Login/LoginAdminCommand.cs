using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Auth;

namespace PortalLioConnecta.Api.Features.Admin.Auth.Login;

public sealed record LoginAdminCommand(AdminLoginRequest Request) : IRequest<AdminLoginResponse?>;
