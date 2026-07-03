using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Services;

public sealed class HrRmAccessGuard
{
    private readonly ITotvsRmConfigurationService _configurationService;
    private readonly ITotvsRmHrContextService _hrContextService;

    public HrRmAccessGuard(
        ITotvsRmConfigurationService configurationService,
        ITotvsRmHrContextService hrContextService)
    {
        _configurationService = configurationService;
        _hrContextService = hrContextService;
    }

    public async Task<(TotvsRmHrResolution Resolution, TotvsRmRuntimeConfiguration Runtime)> EnsureAsync(
        PortalUser user,
        Func<TotvsRmModuleFlags, bool> moduleEnabled,
        string moduleDisabledMessage,
        CancellationToken cancellationToken)
    {
        var runtime = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!runtime.IsEnabled)
        {
            return (TotvsRmHrResolution.Disabled(
                "Consulta temporariamente indisponivel. Entre em contato com o RH."), runtime);
        }

        if (runtime.ModuleFlags.HasAnyEnabled && !moduleEnabled(runtime.ModuleFlags))
        {
            return (TotvsRmHrResolution.ModuleDisabled(moduleDisabledMessage), runtime);
        }

        var resolution = await _hrContextService.ResolveAsync(user, persistWhenFound: true, cancellationToken);
        return (resolution, runtime);
    }
}
