namespace PortalRH.Api.Domain;

public sealed record PortalShellNavDefinition(
    string Label,
    string Route,
    string ModuleKey,
    string MinimumAccessLevel);

public static class PortalShellNavigationCatalog
{
    public static IReadOnlyList<PortalShellNavDefinition> GetDefinitions()
        =>
        [
            new("INICIO", "inicio", PortalModulePermissionCatalog.Home, PortalModulePermissionCatalog.View),
            new("COMUNICACAO", "comunicacao", PortalModulePermissionCatalog.Communications, PortalModulePermissionCatalog.View),
            new("ENQUETES", "enquetes", PortalModulePermissionCatalog.Polls, PortalModulePermissionCatalog.View),
            new("PESSOAS (RH)", "pessoas-rh", PortalModulePermissionCatalog.HrProfile, PortalModulePermissionCatalog.View),
            new("SISTEMAS", "sistemas", PortalModulePermissionCatalog.Home, PortalModulePermissionCatalog.View),
            new("PROJETOS", "projetos", PortalModulePermissionCatalog.Home, PortalModulePermissionCatalog.View),
            new("RECURSOS", "recursos", PortalModulePermissionCatalog.Home, PortalModulePermissionCatalog.View),
            new("CONFIGURACOES", "configuracoes", PortalModulePermissionCatalog.Settings, PortalModulePermissionCatalog.Manage)
        ];
}
