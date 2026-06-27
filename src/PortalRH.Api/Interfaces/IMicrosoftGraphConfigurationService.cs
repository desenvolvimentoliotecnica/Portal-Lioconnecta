using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Services;

namespace PortalRH.Api.Interfaces;

public interface IMicrosoftGraphConfigurationService
{
    Task<MicrosoftGraphConfigurationDto> GetAsync(CancellationToken cancellationToken);
    Task<MicrosoftGraphConfigurationDto> SaveAsync(UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken);
    Task<MicrosoftGraphRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken);
    Task<MicrosoftGraphConnectionTestResponse> TestConnectionAsync(UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken);
    Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken);
}
