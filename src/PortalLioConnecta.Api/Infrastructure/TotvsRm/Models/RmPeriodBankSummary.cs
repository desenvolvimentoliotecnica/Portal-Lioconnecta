namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmPeriodBankSummary
{
    public int? PreviousBalanceMinutes { get; init; }
    public int? PeriodBalanceMinutes { get; init; }
    public int? TotalBalanceMinutes { get; init; }
    public string SourceTable { get; init; } = string.Empty;
}
