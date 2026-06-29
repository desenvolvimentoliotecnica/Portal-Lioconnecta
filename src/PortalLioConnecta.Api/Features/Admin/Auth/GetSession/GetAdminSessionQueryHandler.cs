using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Auth;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.Auth.GetSession;

public class GetAdminSessionQueryHandler : IRequestHandler<GetAdminSessionQuery, AdminSessionDto?>
{
    private readonly IAdminAuthService _adminAuthService;

    public GetAdminSessionQueryHandler(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    public Task<AdminSessionDto?> Handle(GetAdminSessionQuery request, CancellationToken cancellationToken)
    {
        return _adminAuthService.GetActiveSessionAsync(request.Token, cancellationToken);
    }
}
