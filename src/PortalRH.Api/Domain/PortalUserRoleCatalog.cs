namespace PortalRH.Api.Domain;

public sealed record PortalUserRoleDefinition(
    string Key,
    string Label,
    IReadOnlyList<string> Permissions);

public static class PortalUserRoleCatalog
{
    public const string Collaborator = "Collaborator";
    public const string HrManager = "HrManager";
    public const string CommunicationEditor = "CommunicationEditor";
    public const string PortalAdmin = "PortalAdmin";

    private static readonly IReadOnlyList<PortalUserRoleDefinition> Definitions =
    [
        new(
            Collaborator,
            "Colaborador",
            [
                "Acessar mural inicial",
                "Ler comunicados oficiais",
                "Responder enquetes internas",
                "Interagir com o feed interno",
                "Usar atalhos e servicos do portal"
            ]),
        new(
            HrManager,
            "Gestor de RH",
            [
                "Acessar mural inicial",
                "Ler comunicados oficiais",
                "Responder enquetes internas",
                "Interagir com o feed interno",
                "Usar atalhos e servicos do portal",
                "Consultar paineis e atalhos de RH",
                "Gerenciar enquetes internas",
                "Publicar comunicados editoriais"
            ]),
        new(
            CommunicationEditor,
            "Editor de comunicacao",
            [
                "Acessar mural inicial",
                "Ler comunicados oficiais",
                "Responder enquetes internas",
                "Interagir com o feed interno",
                "Usar atalhos e servicos do portal",
                "Publicar comunicados editoriais"
            ]),
        new(
            PortalAdmin,
            "Administrador do portal",
            [
                "Acessar mural inicial",
                "Ler comunicados oficiais",
                "Responder enquetes internas",
                "Interagir com o feed interno",
                "Usar atalhos e servicos do portal",
                "Publicar comunicados editoriais",
                "Gerenciar enquetes internas",
                "Acessar configuracoes administrativas"
            ])
    ];

    public static IReadOnlyList<PortalUserRoleDefinition> GetAll()
        => Definitions;

    public static bool IsValid(string? role)
        => Definitions.Any(item => string.Equals(item.Key, role, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string? role)
        => Definitions.FirstOrDefault(item => string.Equals(item.Key, role, StringComparison.OrdinalIgnoreCase))?.Key
            ?? Collaborator;

    public static string GetLabel(string? role)
        => Definitions.FirstOrDefault(item => string.Equals(item.Key, role, StringComparison.OrdinalIgnoreCase))?.Label
            ?? "Colaborador";

    public static IReadOnlyList<string> GetPermissions(string? role)
        => Definitions.FirstOrDefault(item => string.Equals(item.Key, role, StringComparison.OrdinalIgnoreCase))?.Permissions
            ?? Definitions[0].Permissions;
}
