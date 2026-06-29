namespace PortalLioConnecta.Api.Contracts.Polls;

public record PollOptionDto(
    Guid Id,
    string Label,
    int DisplayOrder,
    int Votes,
    double Percentage,
    bool IsSelected);
