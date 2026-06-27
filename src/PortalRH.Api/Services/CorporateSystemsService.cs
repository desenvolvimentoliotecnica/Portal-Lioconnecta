using PortalRH.Api.Contracts.CorporateSystems;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class CorporateSystemsService : ICorporateSystemsService
{
    public Task<CorporateSystemsResponse> GetSystemsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new CorporateSystemsResponse(
            [
                new CorporateSystemItemDto("Corporativos", "https://portal.example.local/sistemas", "Portal"),
                new CorporateSystemItemDto("Google Workspace", "https://workspace.google.com", "Google"),
                new CorporateSystemItemDto("SISTEMAS", "#sistemas", "Portal"),
                new CorporateSystemItemDto("PROJETOS", "#projetos", "Portal"),
                new CorporateSystemItemDto("RECURSOS", "#recursos", "Portal")
            ],
            "Catalogo Corporativo",
            true);

        return Task.FromResult(response);
    }
}
