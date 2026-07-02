using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Interfaces;

public interface ITotvsRmConfigurationService
{
    Task<TotvsRmConfigurationDto> GetAsync(CancellationToken cancellationToken);
    Task<TotvsRmConfigurationDto> SaveAsync(UpsertTotvsRmConfigurationRequest request, CancellationToken cancellationToken);
    Task<TotvsRmRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken);
    Task<TotvsRmConnectionTestResponse> TestConnectionAsync(UpsertTotvsRmConfigurationRequest request, CancellationToken cancellationToken);
    Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken);
}
