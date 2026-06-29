namespace PortalLioConnecta.Api.Contracts.Polls;

public record PollDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Body,
    string? ImageUrl,
    string? AttachmentLabel,
    string? AttachmentUrl,
    string Audience,
    string Status,
    string StatusLabel,
    bool AllowMultipleChoices,
    string ResultsVisibility,
    string ResultsVisibilityLabel,
    bool IsFeatured,
    DateTime? PublishedAtUtc,
    DateTime? ClosesAtUtc,
    int TotalVotes,
    bool HasVoted,
    bool ResultsVisible,
    IReadOnlyList<PollOptionDto> Options);
