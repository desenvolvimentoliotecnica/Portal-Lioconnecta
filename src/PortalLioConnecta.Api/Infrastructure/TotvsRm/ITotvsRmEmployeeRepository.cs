using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public interface ITotvsRmEmployeeRepository
{
    Task<string?> LookupChapaByFullNameAsync(string fullName, CancellationToken cancellationToken);
    Task<TotvsRmHrContext?> GetHrContextByChapaAsync(string chapa, CancellationToken cancellationToken);
    Task<RmEmployeeProfileRecord?> GetProfileByChapaAsync(string chapa, CancellationToken cancellationToken);
}
