using System.Text.Json;

namespace PortalRH.Api.Domain;

public sealed record PortalModuleDefinition(string Key, string Label);

public sealed record PortalModuleAccessLevelDefinition(string Key, string Label);

public sealed record PortalModulePermissionAssignment(string ModuleKey, string AccessLevel);

public static class PortalModulePermissionCatalog
{
    public const string None = "None";
    public const string View = "View";
    public const string Interact = "Interact";
    public const string Manage = "Manage";

    public const string Home = "home";
    public const string Communications = "communications";
    public const string Feed = "feed";
    public const string QuickLinks = "quick-links";
    public const string Agenda = "agenda";
    public const string HrProfile = "hr-profile";
    public const string Polls = "polls";
    public const string CommunicationAdmin = "communication-admin";
    public const string PollAdmin = "poll-admin";
    public const string Settings = "settings";
    public const string UserAdmin = "user-admin";
    public const string Audit = "audit";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyList<PortalModuleDefinition> Modules =
    [
        new(Home, "Home"),
        new(Communications, "Comunicados"),
        new(Feed, "Feed social"),
        new(QuickLinks, "Acessos rapidos"),
        new(Agenda, "Agenda"),
        new(HrProfile, "Painel RH"),
        new(Polls, "Enquetes"),
        new(CommunicationAdmin, "Editorial"),
        new(PollAdmin, "Enquetes (admin)"),
        new(Settings, "Configuracoes"),
        new(UserAdmin, "Usuarios"),
        new(Audit, "Auditoria")
    ];

    private static readonly IReadOnlyList<PortalModuleAccessLevelDefinition> AccessLevels =
    [
        new(None, "Sem acesso"),
        new(View, "Visualizar"),
        new(Interact, "Interagir"),
        new(Manage, "Gerenciar")
    ];

    public static IReadOnlyList<PortalModuleDefinition> GetModules()
        => Modules;

    public static IReadOnlyList<PortalModuleAccessLevelDefinition> GetAccessLevels()
        => AccessLevels;

    public static string NormalizeModuleKey(string? moduleKey)
        => Modules.FirstOrDefault(item => string.Equals(item.Key, moduleKey, StringComparison.OrdinalIgnoreCase))?.Key
            ?? Home;

    public static string NormalizeAccessLevel(string? accessLevel)
        => AccessLevels.FirstOrDefault(item => string.Equals(item.Key, accessLevel, StringComparison.OrdinalIgnoreCase))?.Key
            ?? None;

    public static string GetModuleLabel(string? moduleKey)
        => Modules.FirstOrDefault(item => string.Equals(item.Key, moduleKey, StringComparison.OrdinalIgnoreCase))?.Label
            ?? "Modulo";

    public static string GetAccessLevelLabel(string? accessLevel)
        => AccessLevels.FirstOrDefault(item => string.Equals(item.Key, accessLevel, StringComparison.OrdinalIgnoreCase))?.Label
            ?? "Sem acesso";

    public static IReadOnlyList<PortalModulePermissionAssignment> GetDefaultAssignments(string? role)
    {
        var normalizedRole = PortalUserRoleCatalog.Normalize(role);

        return normalizedRole switch
        {
            PortalUserRoleCatalog.HrManager =>
                CreateAssignments(
                    View,
                    [Feed, Polls],
                    [HrProfile, PollAdmin, CommunicationAdmin]),
            PortalUserRoleCatalog.CommunicationEditor =>
                CreateAssignments(
                    View,
                    [Feed, Polls],
                    [Communications, CommunicationAdmin]),
            PortalUserRoleCatalog.PortalAdmin =>
                CreateAssignments(
                    Manage,
                    [],
                    [Communications, Feed, CommunicationAdmin, Polls, PollAdmin, Settings, UserAdmin, Audit, HrProfile]),
            _ =>
                CreateAssignments(
                    View,
                    [Feed, Polls],
                    [])
        };
    }

    public static IReadOnlyList<PortalModulePermissionAssignment> DeserializeOrDefault(string? json, string? role)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return GetDefaultAssignments(role);
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<PortalModulePermissionAssignment>>(json, SerializerOptions);
            return ReconcileWithRoleDefaults(NormalizeAssignments(items, role), role);
        }
        catch
        {
            return GetDefaultAssignments(role);
        }
    }

    public static string Serialize(IEnumerable<PortalModulePermissionAssignment> assignments, string? role)
    {
        var normalized = NormalizeAssignments(assignments, role);
        return JsonSerializer.Serialize(normalized, SerializerOptions);
    }

    public static IReadOnlyList<PortalModulePermissionAssignment> NormalizeAssignments(IEnumerable<PortalModulePermissionAssignment>? assignments, string? role)
    {
        var defaults = GetDefaultAssignments(role).ToDictionary(item => item.ModuleKey, item => item.AccessLevel, StringComparer.OrdinalIgnoreCase);
        if (assignments is not null)
        {
            foreach (var assignment in assignments)
            {
                defaults[NormalizeModuleKey(assignment.ModuleKey)] = NormalizeAccessLevel(assignment.AccessLevel);
            }
        }

        return Modules
            .Select(item => new PortalModulePermissionAssignment(
                item.Key,
                defaults.TryGetValue(item.Key, out var accessLevel)
                    ? NormalizeAccessLevel(accessLevel)
                    : None))
            .ToList();
    }

    public static IReadOnlyList<string> ToSummaryLabels(IEnumerable<PortalModulePermissionAssignment> assignments)
    {
        return NormalizeAssignments(assignments, PortalUserRoleCatalog.Collaborator)
            .Where(item => !string.Equals(item.AccessLevel, None, StringComparison.OrdinalIgnoreCase))
            .Select(item => $"{GetModuleLabel(item.ModuleKey)}: {GetAccessLevelLabel(item.AccessLevel)}")
            .ToList();
    }

    private static IReadOnlyList<PortalModulePermissionAssignment> CreateAssignments(
        string baseAccessLevel,
        IReadOnlyCollection<string> interactiveModules,
        IReadOnlyCollection<string> managedModules)
    {
        return Modules
            .Select(item =>
            {
                if (managedModules.Contains(item.Key, StringComparer.OrdinalIgnoreCase))
                {
                    return new PortalModulePermissionAssignment(item.Key, Manage);
                }

                if (interactiveModules.Contains(item.Key, StringComparer.OrdinalIgnoreCase))
                {
                    return new PortalModulePermissionAssignment(item.Key, Interact);
                }

                return new PortalModulePermissionAssignment(item.Key, baseAccessLevel);
            })
            .ToList();
    }

    private static readonly IReadOnlyDictionary<string, int> AccessLevelRank =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [None] = 0,
            [View] = 1,
            [Interact] = 2,
            [Manage] = 3
        };

    private static string MaxAccessLevel(string? current, string? roleDefault)
    {
        var currentLevel = NormalizeAccessLevel(current);
        var defaultLevel = NormalizeAccessLevel(roleDefault);
        var currentRank = AccessLevelRank.TryGetValue(currentLevel, out var currentValue) ? currentValue : 0;
        var defaultRank = AccessLevelRank.TryGetValue(defaultLevel, out var defaultValue) ? defaultValue : 0;
        return currentRank >= defaultRank ? currentLevel : defaultLevel;
    }

    private static IReadOnlyList<PortalModulePermissionAssignment> ReconcileWithRoleDefaults(
        IReadOnlyList<PortalModulePermissionAssignment> assignments,
        string? role)
    {
        var roleDefaults = GetDefaultAssignments(role).ToDictionary(
            item => item.ModuleKey,
            item => item.AccessLevel,
            StringComparer.OrdinalIgnoreCase);

        return assignments
            .Select(item => roleDefaults.TryGetValue(item.ModuleKey, out var defaultLevel)
                ? new PortalModulePermissionAssignment(item.ModuleKey, MaxAccessLevel(item.AccessLevel, defaultLevel))
                : item)
            .ToList();
    }
}
