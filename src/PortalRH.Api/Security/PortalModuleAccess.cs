using PortalRH.Api.Domain;
using PortalRH.Api.Models;

namespace PortalRH.Api.Security;

public static class PortalModuleAccess
{
    public static bool HasModuleAccess(PortalUser user, string moduleKey, params string[] allowedLevels)
    {
        if (string.Equals(user.Role, PortalUserRoleCatalog.PortalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var assignments = PortalModulePermissionCatalog.DeserializeOrDefault(user.ModulePermissionsJson, user.Role);
        var assignment = assignments.FirstOrDefault(item =>
            string.Equals(item.ModuleKey, moduleKey, StringComparison.OrdinalIgnoreCase));

        if (assignment is null)
        {
            return false;
        }

        return allowedLevels.Any(level =>
            string.Equals(assignment.AccessLevel, level, StringComparison.OrdinalIgnoreCase));
    }

    public static bool CanViewHrDashboard(PortalUser user)
    {
        if (string.Equals(user.Role, PortalUserRoleCatalog.PortalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HasModuleAccess(
            user,
            PortalModulePermissionCatalog.HrProfile,
            PortalModulePermissionCatalog.Interact,
            PortalModulePermissionCatalog.Manage);
    }

    public static bool CanManagePolls(PortalUser user)
    {
        if (string.Equals(user.Role, PortalUserRoleCatalog.PortalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(user.Role, PortalUserRoleCatalog.HrManager, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (HasModuleAccess(
                user,
                PortalModulePermissionCatalog.PollAdmin,
                PortalModulePermissionCatalog.Manage))
        {
            return true;
        }

        return HasModuleAccess(
            user,
            PortalModulePermissionCatalog.HrProfile,
            PortalModulePermissionCatalog.Manage);
    }

    public static bool CanManageMoodSurveyFeedback(PortalUser user)
    {
        if (string.Equals(user.Role, PortalUserRoleCatalog.PortalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(user.Role, PortalUserRoleCatalog.HrManager, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HasModuleAccess(
            user,
            PortalModulePermissionCatalog.HrProfile,
            PortalModulePermissionCatalog.Manage);
    }

    public static bool CanManageCommunications(PortalUser user)
    {
        if (string.Equals(user.Role, PortalUserRoleCatalog.PortalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(user.Role, PortalUserRoleCatalog.HrManager, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(user.Role, PortalUserRoleCatalog.CommunicationEditor, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (HasModuleAccess(
                user,
                PortalModulePermissionCatalog.CommunicationAdmin,
                PortalModulePermissionCatalog.Manage))
        {
            return true;
        }

        return HasModuleAccess(
            user,
            PortalModulePermissionCatalog.HrProfile,
            PortalModulePermissionCatalog.Manage);
    }
}
