namespace PortalLioConnecta.Web.Models.CareerTimeline;

public sealed class CareerTimelineShowcaseViewModel
{
    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required IReadOnlyList<TimelineSummaryCardViewModel> SummaryCards { get; init; }

    public required IReadOnlyList<TimelineMilestoneViewModel> Milestones { get; init; }

    public required IReadOnlyList<CareerTimelineTrackRowViewModel> TrackRows { get; init; }

    public required IReadOnlyList<TimelineLegendItemViewModel> LegendItems { get; init; }
}
