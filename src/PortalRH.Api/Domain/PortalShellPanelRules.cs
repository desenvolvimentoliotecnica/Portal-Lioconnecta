namespace PortalRH.Api.Domain;

public static class PortalShellPanelRules
{
    public static string ResolveModuleKey(PanelShellDescriptor descriptor)
    {
        if (!string.IsNullOrWhiteSpace(descriptor.ModuleKey))
        {
            return PortalModulePermissionCatalog.NormalizeModuleKey(descriptor.ModuleKey);
        }

        if (string.Equals(descriptor.Type, "quick-links", StringComparison.OrdinalIgnoreCase))
        {
            return PortalModulePermissionCatalog.QuickLinks;
        }

        if (string.Equals(descriptor.Type, "profile", StringComparison.OrdinalIgnoreCase))
        {
            return PortalModulePermissionCatalog.HrProfile;
        }

        return descriptor.Title.ToUpperInvariant() switch
        {
            "AGENDA" or "AGENDA DO DIA" => PortalModulePermissionCatalog.Agenda,
            "COMUNICADOS" => PortalModulePermissionCatalog.Communications,
            _ => PortalModulePermissionCatalog.Home
        };
    }
}

public sealed record PanelShellDescriptor(string Type, string Title, string ModuleKey);
