using MediatR;

namespace PortalLioConnecta.Api.Features.Admin.Auth.Logout;

public sealed record LogoutAdminCommand(string Token) : IRequest<bool>;
