namespace PortalRH.Api.Contracts.Kpis;

public sealed record KpiItemDto(
    string Label,
    string Value,
    string Source);

public sealed record KpiSummaryResponse(
    IReadOnlyList<KpiItemDto> Items,
    string Provider,
    bool IsSimulated);
