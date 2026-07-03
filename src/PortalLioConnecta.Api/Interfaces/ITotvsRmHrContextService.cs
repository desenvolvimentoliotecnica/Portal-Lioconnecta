using PortalLioConnecta.Api.Models;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Interfaces;

public interface ITotvsRmHrContextService
{
    Task<TotvsRmHrResolution> ResolveAsync(
        PortalUser user,
        bool persistWhenFound,
        CancellationToken cancellationToken);
}
