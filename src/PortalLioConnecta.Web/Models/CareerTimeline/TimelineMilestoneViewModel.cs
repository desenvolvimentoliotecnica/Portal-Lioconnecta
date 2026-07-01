namespace PortalLioConnecta.Web.Models.CareerTimeline;

public sealed class TimelineMilestoneViewModel
{
    public required int Step { get; init; }

    public required DateTime DateValue { get; init; }

    public required string DateText { get; init; }

    public required string EventText { get; init; }

    public required string SalaryText { get; init; }

    public required string AccentClass { get; init; }

    public required TimelineIconKind Icon { get; init; }
}
