using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public interface ITotvsRmPayrollRepository
{
    Task<IReadOnlyList<RmPayslipSummaryRecord>> GetPayslipSummariesAsync(string chapa, int maxEnvelopes, CancellationToken cancellationToken);
    Task<IReadOnlyList<RmPayslipLineRecord>> GetPayslipLinesAsync(string chapa, int anoComp, int mesComp, int nroPeriodo, CancellationToken cancellationToken);
    Task<RmPayslipPeriodRecord?> GetPayslipPeriodAsync(string chapa, int anoComp, int mesComp, int nroPeriodo, CancellationToken cancellationToken);
}

public interface ITotvsRmVacationRepository
{
    Task<RmVacationBalanceRecord?> GetBalanceAsync(string chapa, CancellationToken cancellationToken);
    Task<IReadOnlyList<RmVacationPeriodRecord>> GetPeriodsAsync(string chapa, CancellationToken cancellationToken);
}

public interface ITotvsRmBenefitsRepository
{
    Task<IReadOnlyList<RmBenefitRecord>> GetBenefitsAsync(string chapa, CancellationToken cancellationToken);
    Task<IReadOnlyList<RmDependentRecord>> GetDependentsAsync(string chapa, CancellationToken cancellationToken);
}

public interface ITotvsRmTeamRepository
{
    Task<IReadOnlyList<RmTeamMemberRecord>> GetTeamMembersAsync(string codSecao, CancellationToken cancellationToken);
}
