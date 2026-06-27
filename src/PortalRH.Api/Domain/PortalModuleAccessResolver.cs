using PortalRH.Api.Models;

namespace PortalRH.Api.Domain;

public static class PortalModuleAccessResolver
{
    public static string GetAccessLevel(PortalUser user, string moduleKey)
    {
        var assignments = PortalModulePermissionCatalog.DeserializeOrDefault(user.ModulePermissionsJson, user.Role);
        return assignments.FirstOrDefault(item => string.Equals(item.ModuleKey, moduleKey, StringComparison.OrdinalIgnoreCase))?.AccessLevel
            ?? PortalModulePermissionCatalog.None;
    }

    public static bool HasAtLeast(PortalUser user, string moduleKey, string minimumAccessLevel)
    {
        return Rank(GetAccessLevel(user, moduleKey)) >= Rank(minimumAccessLevel);
    }

    private static int Rank(string? accessLevel)
    {
        return PortalModulePermissionCatalog.NormalizeAccessLevel(accessLevel) switch
        {
            PortalModulePermissionCatalog.Manage => 3,
            PortalModulePermissionCatalog.Interact => 2,
            PortalModulePermissionCatalog.View => 1,
            _ => 0
        };
    }
}
