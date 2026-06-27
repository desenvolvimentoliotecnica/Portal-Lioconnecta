namespace PortalRH.Api.Contracts.CorporateSystems;

public sealed record CorporateSystemItemDto(string Label, string Url, string Provider);

public sealed record CorporateSystemsResponse(
    IReadOnlyList<CorporateSystemItemDto> Items,
    string Provider,
    bool IsSimulated);
