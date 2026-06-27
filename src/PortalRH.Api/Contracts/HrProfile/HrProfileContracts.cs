namespace PortalRH.Api.Contracts.HrProfile;

public sealed record HrProfileItemDto(
    string Label,
    string Url,
    string Provider,
    bool IsExternal);

public sealed record HrProfileResponse(
    string Name,
    string Subtitle,
    string Description,
    string ManagerDisplayName,
    IReadOnlyList<HrProfileItemDto> Items,
    string Provider,
    bool IsSimulated);
