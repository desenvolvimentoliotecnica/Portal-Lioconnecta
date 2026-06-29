using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Interfaces;

public interface IMicrosoftGraphConfigurationService
{
    Task<MicrosoftGraphConfigurationDto> GetAsync(CancellationToken cancellationToken);
    Task<MicrosoftGraphConfigurationDto> SaveAsync(UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken);
    Task<MicrosoftGraphRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken);
    Task<MicrosoftGraphConnectionTestResponse> TestConnectionAsync(UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken);
    Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken);
}
