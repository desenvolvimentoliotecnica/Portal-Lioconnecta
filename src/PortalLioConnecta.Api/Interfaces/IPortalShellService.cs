using PortalLioConnecta.Api.Contracts.Shell;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IPortalShellService
{
    Task<MeUiResponse> BuildMeUiAsync(PortalUser user, CancellationToken cancellationToken);

    Task<PanelsResponse> BuildPanelsAsync(PortalUser user, CancellationToken cancellationToken);
}
