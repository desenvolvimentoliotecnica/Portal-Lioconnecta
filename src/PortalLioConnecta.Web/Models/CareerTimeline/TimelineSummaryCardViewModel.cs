namespace PortalLioConnecta.Web.Models.CareerTimeline;

public sealed class TimelineSummaryCardViewModel
{
    public required string Title { get; init; }

    public required string Value { get; init; }

    public required string AccentClass { get; init; }

    public required TimelineIconKind Icon { get; init; }
}
