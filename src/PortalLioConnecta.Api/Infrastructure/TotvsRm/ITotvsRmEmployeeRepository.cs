namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public interface ITotvsRmEmployeeRepository
{
    Task<string?> LookupChapaByFullNameAsync(string fullName, CancellationToken cancellationToken);
}
