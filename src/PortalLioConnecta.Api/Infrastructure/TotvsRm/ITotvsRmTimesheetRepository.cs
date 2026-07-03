using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public interface ITotvsRmTimesheetRepository
{
    Task<IReadOnlyList<RmPunchRecord>> GetPunchesAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RmProcessedDayRecord>> GetProcessedDaysAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken);

    Task<RmPeriodBankSummary?> GetPeriodBankSummaryAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken);
}
