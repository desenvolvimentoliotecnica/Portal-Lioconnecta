namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public record PollAdminListResponse(
    IReadOnlyList<PollAdminDto> Items,
    PollAdminSummaryDto Summary);
