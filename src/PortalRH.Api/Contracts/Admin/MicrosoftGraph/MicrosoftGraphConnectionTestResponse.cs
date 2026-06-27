namespace PortalRH.Api.Contracts.Admin.MicrosoftGraph;

public sealed record MicrosoftGraphConnectionTestResponse(
    bool Success,
    string Message,
    string? Detail);
