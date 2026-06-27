namespace PortalRH.Api.Contracts.QuickLinks;

public sealed record QuickLinkDto(
    Guid Id,
    string Label,
    string ShortLabel,
    string ClassName,
    string Url);

public sealed record QuickLinkListResponse(IReadOnlyList<QuickLinkDto> Items);
