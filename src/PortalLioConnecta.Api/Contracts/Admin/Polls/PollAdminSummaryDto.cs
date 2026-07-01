namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public record PollAdminSummaryDto(
    int TotalPolls,
    int PublishedPolls,
    int DraftPolls,
    int ClosedPolls,
    int ArchivedPolls,
    int TotalVotes);
