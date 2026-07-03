using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Interfaces;

public interface ILdapDirectoryAuthenticator
{
    Task<LdapAuthenticatedUser?> AuthenticateAsync(
        LdapRuntimeConfiguration configuration,
        string login,
        string password,
        CancellationToken cancellationToken);

    Task<LdapAuthenticatedUser?> LookupUserProfileAsync(
        LdapRuntimeConfiguration configuration,
        IEnumerable<string> lookupCandidates,
        string? distinguishedName,
        CancellationToken cancellationToken);
}
