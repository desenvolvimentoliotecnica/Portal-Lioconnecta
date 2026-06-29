using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Auth;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.Auth.Login;

public class LoginAdminCommandHandler : IRequestHandler<LoginAdminCommand, AdminLoginResponse?>
{
    private readonly IAdminAuthService _adminAuthService;

    public LoginAdminCommandHandler(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    public Task<AdminLoginResponse?> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
    {
        return _adminAuthService.LoginAsync(request.Request, cancellationToken);
    }
}
