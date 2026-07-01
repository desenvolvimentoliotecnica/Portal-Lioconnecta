using PortalLioConnecta.Api.Contracts.Shell;
using PortalLioConnecta.Api.Domain;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class PortalShellService : IPortalShellService
{
    private readonly IPortalPanelsComposer _portalPanelsComposer;
    private readonly IMicrosoftGraphUserPhotoService _userPhotoService;

    public PortalShellService(
        IPortalPanelsComposer portalPanelsComposer,
        IMicrosoftGraphUserPhotoService userPhotoService)
    {
        _portalPanelsComposer = portalPanelsComposer;
        _userPhotoService = userPhotoService;
    }

    public async Task<MeUiResponse> BuildMeUiAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var template = PortalShellDefaults.CreateMeUiTemplate();
        var photoUrl = await _userPhotoService.GetPhotoDataUrlForPortalUserAsync(user, cancellationToken) ?? string.Empty;

        return template with
        {
            User = template.User with
            {
                Name = user.DisplayName,
                Area = user.Department ?? string.Empty,
                NotificationCount = 0,
                PhotoUrl = photoUrl
            },
            NavItems = BuildNavItems(user),
            Composer = template.Composer with
            {
                Enabled = PortalModuleAccessResolver.HasAtLeast(
                    user,
                    PortalModulePermissionCatalog.Feed,
                    PortalModulePermissionCatalog.Interact)
            }
        };
    }

    public Task<PanelsResponse> BuildPanelsAsync(PortalUser user, CancellationToken cancellationToken)
        => _portalPanelsComposer.BuildAsync(user, cancellationToken);

    private static IReadOnlyList<NavItemDto> BuildNavItems(PortalUser user)
    {
        return PortalShellNavigationCatalog.GetDefinitions()
            .Where(item => PortalModuleAccessResolver.HasAtLeast(user, item.ModuleKey, item.MinimumAccessLevel))
            .Select(item => new NavItemDto(item.Label, item.Route, item.ModuleKey, false))
            .ToList();
    }
}
