namespace PortalLioConnecta.Web.Models.AntDesignShowcase;

public class AntDesignHrShowcaseViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public IReadOnlyList<AntDesignMetricViewModel> Metrics { get; init; } = Array.Empty<AntDesignMetricViewModel>();
    public IReadOnlyList<AntDesignCandidateViewModel> Candidates { get; init; } = Array.Empty<AntDesignCandidateViewModel>();
    public IReadOnlyList<AntDesignTimelineEventViewModel> TimelineEvents { get; init; } = Array.Empty<AntDesignTimelineEventViewModel>();
    public IReadOnlyList<AntDesignVacancyViewModel> Vacancies { get; init; } = Array.Empty<AntDesignVacancyViewModel>();
    public IReadOnlyList<AntDesignStageViewModel> Stages { get; init; } = Array.Empty<AntDesignStageViewModel>();
    public IReadOnlyList<AntDesignSummaryCardViewModel> SummaryCards { get; init; } = Array.Empty<AntDesignSummaryCardViewModel>();
    public IReadOnlyList<AntDesignApprovalViewModel> Approvals { get; init; } = Array.Empty<AntDesignApprovalViewModel>();
    public IReadOnlyList<AntDesignIntegrationCheckpointViewModel> IntegrationCheckpoints { get; init; } = Array.Empty<AntDesignIntegrationCheckpointViewModel>();
    public IReadOnlyList<AntDesignActivityViewModel> Activities { get; init; } = Array.Empty<AntDesignActivityViewModel>();
    public IReadOnlyList<AntDesignFilterChipViewModel> Filters { get; init; } = Array.Empty<AntDesignFilterChipViewModel>();
}

public class AntDesignMetricViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string AccentColor { get; init; } = "blue";
}

public class AntDesignCandidateViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string Stage { get; init; } = string.Empty;
    public int Score { get; init; }
    public string TagColor { get; init; } = "blue";
}

public class AntDesignTimelineEventViewModel
{
    public string DateLabel { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = "blue";
}

public class AntDesignVacancyViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string HiringManager { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusColor { get; init; } = "blue";
}

public class AntDesignStageViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public int Percent { get; init; }
    public string Status { get; init; } = "Normal";
}

public class AntDesignSummaryCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TagText { get; init; } = string.Empty;
    public string TagColor { get; init; } = "blue";
}

public class AntDesignApprovalViewModel
{
    public string RequestTitle { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string PriorityColor { get; init; } = "blue";
    public string Eta { get; init; } = string.Empty;
}

public class AntDesignIntegrationCheckpointViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public bool Done { get; init; }
}

public class AntDesignActivityViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TimeLabel { get; init; } = string.Empty;
}

public class AntDesignFilterChipViewModel
{
    public string Label { get; init; } = string.Empty;
    public string Color { get; init; } = "blue";
}
