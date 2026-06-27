namespace PortalRH.Api.Contracts.Feed;

public record FeedPostCommentMentionDto(
    Guid UserId,
    string DisplayName);

public record FeedPostCommentDto(
    Guid Id,
    string Author,
    string Text,
    DateTime CreatedAtUtc,
    IReadOnlyList<FeedPostCommentMentionDto> Mentions);

public record FeedPostCommentsResponse(
    Guid FeedPostId,
    IReadOnlyList<FeedPostCommentDto> Items);

public record CreateFeedPostCommentRequest
{
    public string Text { get; set; } = string.Empty;
    public List<Guid> MentionedUserIds { get; set; } = [];
}

public record CreateFeedPostCommentResponse(FeedPostCommentDto Item);

public record FeedMentionSuggestionDto(
    Guid UserId,
    string DisplayName,
    string Department);

public record FeedMentionSuggestionsResponse(
    IReadOnlyList<FeedMentionSuggestionDto> Items);

public record FeedMediaItemDto(
    Guid Id,
    string Url,
    string Description,
    string AspectRatio,
    int SortOrder,
    int CommentCount);

public record FeedMediaCommentDto(
    Guid Id,
    string Author,
    string Text,
    DateTime CreatedAtUtc);

public record FeedMediaCommentsResponse(
    Guid MediaId,
    IReadOnlyList<FeedMediaCommentDto> Items);

public record CreateFeedMediaCommentRequest
{
    public string Text { get; set; } = string.Empty;
}

public record CreateFeedMediaCommentResponse(FeedMediaCommentDto Item);

public record FeedItemDto(
    Guid Id,
    string Source,
    Guid? CommunicationId,
    string Author,
    Guid? AuthorUserId,
    string Area,
    DateTime PublishedAtUtc,
    string Text,
    IReadOnlyList<FeedPostCommentMentionDto> Mentions,
    string? HighlightTitle,
    string? HighlightText,
    string? ImageUrl,
    int LikeCount,
    bool HasLiked,
    int ShareCount,
    bool HasShared,
    bool HasSaved,
    IReadOnlyList<FeedMediaItemDto> Media,
    int CommentCount,
    IReadOnlyList<FeedPostCommentDto> Comments);

public record DeleteFeedPostResponse(
    Guid Id,
    bool Deleted);

public record FeedResponse(
    string Title,
    IReadOnlyList<FeedItemDto> Items);

public record CreateFeedPostMediaItem
{
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AspectRatio { get; set; } = "free";
}

public record CreateFeedPostRequest
{
    public string Text { get; set; } = string.Empty;
    public List<CreateFeedPostMediaItem> Media { get; set; } = [];
    public List<Guid> MentionedUserIds { get; set; } = [];
}

public record CreateFeedPostResponse(FeedItemDto Item);

public record FeedAssetUploadResponse(
    string FileName,
    string ContentType,
    long Size,
    string Url);

public record ToggleFeedLikeRequest
{
    public string Source { get; set; } = string.Empty;
}

public record FeedLikeResponse(
    Guid ItemId,
    string Source,
    int LikeCount,
    bool HasLiked);

public record ToggleFeedShareRequest
{
    public string Source { get; set; } = string.Empty;
}

public record FeedShareResponse(
    Guid ItemId,
    string Source,
    int ShareCount,
    bool HasShared);

public record ToggleFeedSaveRequest
{
    public string Source { get; set; } = string.Empty;
}

public record FeedSaveResponse(
    Guid ItemId,
    string Source,
    bool HasSaved);
