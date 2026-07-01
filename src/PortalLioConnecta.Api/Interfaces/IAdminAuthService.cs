using PortalLioConnecta.Api.Contracts.Admin.Auth;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IAdminAuthService
{
    Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken);
    Task<AdminSessionDto?> GetActiveSessionAsync(string token, CancellationToken cancellationToken);
    Task<bool> LogoutAsync(string token, CancellationToken cancellationToken);
    Task<bool> HasActiveSessionAsync(string token, CancellationToken cancellationToken);
    Task EnsureDefaultSuperAdminAsync(CancellationToken cancellationToken);
}
