namespace PortalLioConnecta.Web.Models.CareerTimeline;

public sealed class TimelineLegendItemViewModel
{
    public required string Text { get; init; }

    public required string AccentClass { get; init; }

    public required TimelineIconKind Icon { get; init; }
}
