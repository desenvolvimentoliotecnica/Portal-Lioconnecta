using PortalLioConnecta.Api.Contracts.Auth;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IPortalAuthService
{
    Task<PortalLoginResponse?> LoginWithLdapAsync(LdapLoginRequest request, CancellationToken cancellationToken);
    Task<PortalLoginResponse?> GetSessionAsync(string token, CancellationToken cancellationToken);
    Task<PortalSession?> GetActiveSessionEntityAsync(string token, CancellationToken cancellationToken);
    Task LogoutAsync(string token, CancellationToken cancellationToken);
}
