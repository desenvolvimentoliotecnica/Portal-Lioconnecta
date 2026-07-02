namespace PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

public sealed record TotvsRmConnectionTestResponse(
    bool Success,
    string Message,
    string? Detail);
