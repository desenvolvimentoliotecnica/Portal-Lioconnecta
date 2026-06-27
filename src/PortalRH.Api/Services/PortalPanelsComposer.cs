using System.Text.Json.Nodes;
using PortalRH.Api.Contracts.Notifications;
using PortalRH.Api.Contracts.Shell;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class PortalPanelsComposer : IPortalPanelsComposer
{
    private static readonly IReadOnlyDictionary<string, string> NotificationCategoryLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RH"] = "Comunicados RH",
            ["Corporativo"] = "Comunicados Corporativos",
            ["Tecnologia"] = "Tecnologia",
            ["Politicas"] = "Politicas",
            ["Políticas"] = "Politicas",
            ["Eventos"] = "Eventos",
            ["Enquetes"] = "Enquetes"
        };

    private readonly INotificationService _notificationService;
    private readonly IAgendaService _agendaService;
    private readonly IQuickLinkService _quickLinkService;
    private readonly IJourneyService _journeyService;
    private readonly IKpiService _kpiService;
    private readonly IHrProfileService _hrProfileService;
    private readonly ICorporateSystemsService _corporateSystemsService;
    private readonly IFeedService _feedService;

    public PortalPanelsComposer(
        INotificationService notificationService,
        IAgendaService agendaService,
        IQuickLinkService quickLinkService,
        IJourneyService journeyService,
        IKpiService kpiService,
        IHrProfileService hrProfileService,
        ICorporateSystemsService corporateSystemsService,
        IFeedService feedService)
    {
        _notificationService = notificationService;
        _agendaService = agendaService;
        _quickLinkService = quickLinkService;
        _journeyService = journeyService;
        _kpiService = kpiService;
        _hrProfileService = hrProfileService;
        _corporateSystemsService = corporateSystemsService;
        _feedService = feedService;
    }

    public async Task<PanelsResponse> BuildAsync(PortalUser user, CancellationToken cancellationToken)
    {
        // DbContext nao e thread-safe: consultas ao banco devem ser sequenciais no mesmo request.
        var notifications = await _notificationService.GetForUserAsync(user.Id, cancellationToken);
        var savedCount = await _feedService.GetSavedItemCountAsync(user.Id, cancellationToken);
        var quickLinks = await _quickLinkService.GetActiveAsync(cancellationToken);
        var agenda = await _agendaService.GetTodayAsync(user.Id, cancellationToken);

        var journeyTask = _journeyService.GetSummaryAsync(user, cancellationToken);
        var systemsTask = _corporateSystemsService.GetSystemsAsync(user, cancellationToken);
        var kpisTask = _kpiService.GetSummaryAsync(user, cancellationToken);
        var hrProfileTask = _hrProfileService.GetProfileAsync(user, cancellationToken);

        await Task.WhenAll(journeyTask, systemsTask, kpisTask, hrProfileTask);

        var leftPanels = new List<PanelDto>
        {
            BuildJourneyPanel(await journeyTask),
            BuildNotificationsPanel(notifications, savedCount),
            BuildCorporateSystemsPanel(await systemsTask),
            BuildKpiPanel(await kpisTask)
        };

        var rightPanels = new List<PanelDto>
        {
            BuildQuickLinksPanel(quickLinks),
            BuildProfilePanel(await hrProfileTask),
            BuildAgendaPanel(agenda)
        };

        return new PanelsResponse(
            FilterPanels(leftPanels, user),
            FilterPanels(rightPanels, user));
    }

    private static PanelDto BuildJourneyPanel(Contracts.Journey.JourneySummaryResponse journey)
    {
        var items = journey.Items
            .Select(item => ShellPanelJson.LabelLink(item.Label, item.Url, item.Badge))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto(string.Empty, "MINHA JORNADA", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.Home, items);
    }

    private static PanelDto BuildNotificationsPanel(NotificationListResponse notifications, int savedCount)
    {
        var items = new List<JsonNode>
        {
            ShellPanelJson.LabelBadge("Notificacoes Totais", notifications.Summary.TotalCount.ToString())
        };

        foreach (var (category, count) in notifications.Summary.CategoryCounts)
        {
            var label = NotificationCategoryLabels.TryGetValue(category, out var mapped) ? mapped : category;
            items.Add(ShellPanelJson.LabelBadge(label, count.ToString()));
        }

        if (items.Count == 1 && notifications.Summary.TotalCount > 0)
        {
            items.Add(ShellPanelJson.LabelBadge("Lidas", notifications.Summary.ReadCount.ToString()));
        }

        items.Add(ShellPanelJson.LabelLink(
            "Itens Salvos",
            "#inicio/salvos",
            savedCount > 0 ? savedCount.ToString() : null));

        return new PanelDto(string.Empty, "MEU PAINEL", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.Home, items);
    }

    private static PanelDto BuildCorporateSystemsPanel(Contracts.CorporateSystems.CorporateSystemsResponse systems)
    {
        var items = systems.Items
            .Select(item => ShellPanelJson.LabelOnly(item.Label, item.Url))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto(string.Empty, "SISTEMAS CORPORATIVOS", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.Home, items);
    }

    private static PanelDto BuildKpiPanel(Contracts.Kpis.KpiSummaryResponse kpis)
    {
        var items = kpis.Items
            .Select(item => ShellPanelJson.LabelValue(item.Label, item.Value))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto(string.Empty, "INDICADORES RAPIDOS", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.Home, items);
    }

    private static PanelDto BuildQuickLinksPanel(Contracts.QuickLinks.QuickLinkListResponse quickLinks)
    {
        var items = quickLinks.Items
            .Select(item => ShellPanelJson.QuickLink(item.ClassName, item.Label, item.ShortLabel, item.Url))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto("quick-links", "ACESSOS RAPIDOS", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.QuickLinks, items);
    }

    private static PanelDto BuildProfilePanel(Contracts.HrProfile.HrProfileResponse profile)
    {
        var items = profile.Items
            .Select(item => ShellPanelJson.HrLink(item.Label, item.Url, item.Provider))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto(
            "profile",
            "MEU PERFIL RH",
            profile.Name,
            profile.Subtitle,
            profile.Description,
            profile.ManagerDisplayName,
            PortalModulePermissionCatalog.HrProfile,
            items);
    }

    private static PanelDto BuildAgendaPanel(Contracts.Agenda.AgendaDayResponse agenda)
    {
        var items = agenda.Items
            .Select(item => ShellPanelJson.AgendaPanelItem(
                item.Id,
                item.Title,
                item.TimeLabel,
                item.Description,
                item.Location,
                item.Source,
                item.StartAtUtc,
                item.EndAtUtc,
                item.JoinUrl,
                item.Participants))
            .Cast<JsonNode>()
            .ToList();

        return new PanelDto(string.Empty, "AGENDA", string.Empty, string.Empty, string.Empty, string.Empty, PortalModulePermissionCatalog.Agenda, items);
    }

    private static IReadOnlyList<PanelDto> FilterPanels(IReadOnlyList<PanelDto> panels, PortalUser user)
    {
        return panels
            .Where(panel =>
            {
                var moduleKey = PortalShellPanelRules.ResolveModuleKey(
                    new PanelShellDescriptor(panel.Type, panel.Title, panel.ModuleKey));

                return PortalModuleAccessResolver.HasAtLeast(
                    user,
                    moduleKey,
                    PortalModulePermissionCatalog.View);
            })
            .ToList();
    }
}
