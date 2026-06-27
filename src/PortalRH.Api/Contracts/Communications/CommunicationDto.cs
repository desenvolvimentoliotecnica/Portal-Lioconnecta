namespace PortalRH.Api.Contracts.Communications;

public record CommunicationDto(
    Guid Id,
    string Slug,
    string Category,
    string Priority,
    string Title,
    string Summary,
    string Body,
    string Audience,
    string Channel,
    string Status,
    string AttachmentLabel,
    string Owner,
    string? ImageUrl,
    bool IsFeatured,
    DateTime PublishedAt,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int LikeCount,
    bool HasLiked,
    bool HasSaved);
