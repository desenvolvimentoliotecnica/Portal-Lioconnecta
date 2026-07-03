using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Tests;

public class HrRmAccessGuardTests
{
    [Fact]
    public async Task EnsureAsync_AllowsPontoWhenLegacyModuleFlagsAreUnset()
    {
        var guard = new HrRmAccessGuard(
            new FakeTotvsRmConfigurationService(new TotvsRmRuntimeConfiguration(
                true,
                "sql.local",
                1433,
                "Corpore",
                "reader",
                "secret",
                true,
                1,
                new TotvsRmModuleFlags(false, false, false, false, false, false),
                16,
                15)),
            new FakeTotvsRmHrContextService());

        var user = new PortalUser { Id = Guid.NewGuid(), DisplayName = "Test User" };

        var (resolution, _) = await guard.EnsureAsync(
            user,
            flags => flags.Ponto,
            "Consulta de ponto temporariamente indisponivel.",
            CancellationToken.None);

        Assert.True(resolution.IsSuccess);
        Assert.Equal("ok", resolution.AvailabilityStatus);
    }

    [Fact]
    public async Task EnsureAsync_BlocksPontoWhenModuleExplicitlyDisabled()
    {
        var guard = new HrRmAccessGuard(
            new FakeTotvsRmConfigurationService(new TotvsRmRuntimeConfiguration(
                true,
                "sql.local",
                1433,
                "Corpore",
                "reader",
                "secret",
                true,
                1,
                new TotvsRmModuleFlags(true, true, true, true, false, true),
                16,
                15)),
            new FakeTotvsRmHrContextService());

        var user = new PortalUser { Id = Guid.NewGuid(), DisplayName = "Test User" };

        var (resolution, _) = await guard.EnsureAsync(
            user,
            flags => flags.Ponto,
            "Consulta de ponto temporariamente indisponivel.",
            CancellationToken.None);

        Assert.False(resolution.IsSuccess);
        Assert.Equal("module_disabled", resolution.AvailabilityStatus);
    }

    private sealed class FakeTotvsRmConfigurationService : ITotvsRmConfigurationService
    {
        private readonly TotvsRmRuntimeConfiguration _runtime;

        public FakeTotvsRmConfigurationService(TotvsRmRuntimeConfiguration runtime)
        {
            _runtime = runtime;
        }

        public Task<Contracts.Admin.TotvsRm.TotvsRmConfigurationDto> GetAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<Contracts.Admin.TotvsRm.TotvsRmConfigurationDto> SaveAsync(
            Contracts.Admin.TotvsRm.UpsertTotvsRmConfigurationRequest request,
            CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<TotvsRmRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken) =>
            Task.FromResult(_runtime);

        public Task<Contracts.Admin.TotvsRm.TotvsRmConnectionTestResponse> TestConnectionAsync(
            Contracts.Admin.TotvsRm.UpsertTotvsRmConfigurationRequest request,
            CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeTotvsRmHrContextService : ITotvsRmHrContextService
    {
        public Task<TotvsRmHrResolution> ResolveAsync(
            PortalUser user,
            bool persistWhenFound,
            CancellationToken cancellationToken) =>
            Task.FromResult(TotvsRmHrResolution.Ok(new TotvsRmHrContext("00000581", null, null), "pfunc"));
    }
}
