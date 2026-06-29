namespace PortalLioConnecta.Web.Models.CareerTimeline;

public sealed class CareerTimelineTrackRowViewModel
{
    public required IReadOnlyList<TimelineMilestoneViewModel> Milestones { get; init; }

    public required IReadOnlyList<TimelineGapViewModel> Gaps { get; init; }
}
