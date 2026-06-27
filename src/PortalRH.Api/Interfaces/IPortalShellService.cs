using PortalRH.Api.Contracts.Shell;
using PortalRH.Api.Models;

namespace PortalRH.Api.Interfaces;

public interface IPortalShellService
{
    Task<MeUiResponse> BuildMeUiAsync(PortalUser user, CancellationToken cancellationToken);

    Task<PanelsResponse> BuildPanelsAsync(PortalUser user, CancellationToken cancellationToken);
}
