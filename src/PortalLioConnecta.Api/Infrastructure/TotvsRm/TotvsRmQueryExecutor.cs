using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public sealed class TotvsRmQueryExecutor
{
    private readonly ITotvsRmConfigurationService _configurationService;
    private readonly ILogger<TotvsRmQueryExecutor> _logger;

    public TotvsRmQueryExecutor(
        ITotvsRmConfigurationService configurationService,
        ILogger<TotvsRmQueryExecutor> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<TotvsRmRuntimeConfiguration> GetRuntimeAsync(CancellationToken cancellationToken) =>
        await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);

    public async Task<T?> TryQueryAsync<T>(
        string operationLabel,
        Func<TotvsRmRuntimeConfiguration, SqlConnection, CancellationToken, Task<T?>> queryFactory,
        CancellationToken cancellationToken,
        bool throwWhenDisabled = true)
        where T : class
    {
        var runtime = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!runtime.IsEnabled)
        {
            if (throwWhenDisabled)
            {
                throw new TotvsRmIntegrationDisabledException();
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(runtime.Password))
        {
            if (throwWhenDisabled)
            {
                throw new TotvsRmIntegrationMisconfiguredException("Credenciais TOTVS RM incompletas.");
            }

            return null;
        }

        try
        {
            await using var connection = TotvsRmConnectionFactory.CreateConnection(runtime);
            await connection.OpenAsync(cancellationToken);
            return await queryFactory(runtime, connection, cancellationToken);
        }
        catch (TotvsRmIntegrationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao consultar TOTVS RM ({OperationLabel}).", operationLabel);

            if (throwWhenDisabled)
            {
                throw new TotvsRmIntegrationUnavailableException();
            }

            return null;
        }
    }

    public async Task<T> QueryAsync<T>(
        string operationLabel,
        Func<TotvsRmRuntimeConfiguration, SqlConnection, CancellationToken, Task<T>> queryFactory,
        CancellationToken cancellationToken)
    {
        var runtime = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!runtime.IsEnabled)
        {
            throw new TotvsRmIntegrationDisabledException();
        }

        if (string.IsNullOrWhiteSpace(runtime.Password))
        {
            throw new TotvsRmIntegrationMisconfiguredException("Credenciais TOTVS RM incompletas.");
        }

        try
        {
            await using var connection = TotvsRmConnectionFactory.CreateConnection(runtime);
            await connection.OpenAsync(cancellationToken);
            return await queryFactory(runtime, connection, cancellationToken);
        }
        catch (TotvsRmIntegrationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao consultar TOTVS RM ({OperationLabel}).", operationLabel);
            throw new TotvsRmIntegrationUnavailableException();
        }
    }
}
