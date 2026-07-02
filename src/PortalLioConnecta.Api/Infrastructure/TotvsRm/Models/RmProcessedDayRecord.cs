namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmProcessedDayRecord
{
    public DateTime DataPonto { get; init; }
    public int? WorkedMinutes { get; init; }
    public int? ExpectedMinutes { get; init; }
    public int? BalanceMinutes { get; init; }
    public int? DelayMinutes { get; init; }
    public int? AbsenceMinutes { get; init; }
    public string? StatusCode { get; init; }
}
