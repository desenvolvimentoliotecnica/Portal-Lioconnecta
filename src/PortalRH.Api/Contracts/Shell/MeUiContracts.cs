namespace PortalRH.Api.Contracts.Shell;

public sealed record BrandDto(string Name, string Tagline);

public sealed record UserSummaryDto(
    string Name,
    string Greeting,
    string Area,
    int NotificationCount,
    string PhotoUrl = "");

public sealed record NavItemDto(
    string Label,
    string Route,
    string ModuleKey,
    bool Active);

public sealed record HeroDto(string Title, string Subtitle);

public sealed record MoodItemDto(string Emoji, string Label, string Rank);

public sealed record MoodDto(string Title, IReadOnlyList<MoodItemDto> Items);

public sealed record ComposerDto(
    bool Enabled,
    string Title,
    string Placeholder,
    IReadOnlyList<string> Actions);

public sealed record MeUiResponse(
    BrandDto Brand,
    UserSummaryDto User,
    IReadOnlyList<NavItemDto> NavItems,
    HeroDto Hero,
    MoodDto Mood,
    ComposerDto Composer);
