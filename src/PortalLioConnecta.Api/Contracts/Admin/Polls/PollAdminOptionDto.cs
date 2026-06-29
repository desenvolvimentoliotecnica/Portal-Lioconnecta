namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public record PollAdminOptionDto(
    Guid Id,
    string Label,
    int DisplayOrder,
    int Votes);
