using MediatR;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.Auth.Logout;

public class LogoutAdminCommandHandler : IRequestHandler<LogoutAdminCommand, bool>
{
    private readonly IAdminAuthService _adminAuthService;

    public LogoutAdminCommandHandler(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    public Task<bool> Handle(LogoutAdminCommand request, CancellationToken cancellationToken)
    {
        return _adminAuthService.LogoutAsync(request.Token, cancellationToken);
    }
}
