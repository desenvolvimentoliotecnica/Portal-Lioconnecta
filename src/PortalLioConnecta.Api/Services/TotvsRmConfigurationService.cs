using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace PortalLioConnecta.Api.Services;

public class TotvsRmConfigurationService : ITotvsRmConfigurationService
{
    private readonly PortalLioConnectaDbContext _dbContext;
    private readonly TotvsRmConnectionTester _connectionTester;
    private readonly ILogger<TotvsRmConfigurationService> _logger;

    public TotvsRmConfigurationService(
        PortalLioConnectaDbContext dbContext,
        TotvsRmConnectionTester connectionTester,
        ILogger<TotvsRmConfigurationService> logger)
    {
        _dbContext = dbContext;
        _connectionTester = connectionTester;
        _logger = logger;
    }

    public async Task<TotvsRmConfigurationDto> GetAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<TotvsRmConfigurationDto> SaveAsync(UpsertTotvsRmConfigurationRequest request, CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);

        entity.IsEnabled = request.IsEnabled;
        entity.Server = Normalize(request.Server);
        entity.Port = request.Port is > 0 and <= 65535 ? request.Port : 1433;
        entity.Database = Normalize(request.Database);
        entity.UserName = Normalize(request.UserName);
        entity.TrustServerCertificate = request.TrustServerCertificate;
        entity.CodColigada = request.CodColigada > 0 ? request.CodColigada : (short)1;
        entity.EnableCadastro = request.EnableCadastro;
        entity.EnableHolerite = request.EnableHolerite;
        entity.EnableFerias = request.EnableFerias;
        entity.EnableBeneficios = request.EnableBeneficios;
        entity.EnablePonto = request.EnablePonto;
        entity.EnableTeamDashboard = request.EnableTeamDashboard;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            entity.PasswordProtected = ProtectSecret(request.Password.Trim());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<TotvsRmRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return new TotvsRmRuntimeConfiguration(
            entity.IsEnabled,
            entity.Server,
            entity.Port,
            entity.Database,
            entity.UserName,
            TryUnprotectSecret(entity),
            entity.TrustServerCertificate,
            entity.CodColigada > 0 ? entity.CodColigada : (short)1,
            new TotvsRmModuleFlags(
                entity.EnableCadastro,
                entity.EnableHolerite,
                entity.EnableFerias,
                entity.EnableBeneficios,
                entity.EnablePonto,
                entity.EnableTeamDashboard));
    }

    public async Task<TotvsRmConnectionTestResponse> TestConnectionAsync(
        UpsertTotvsRmConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        var password = !string.IsNullOrWhiteSpace(request.Password)
            ? request.Password.Trim()
            : TryUnprotectSecret(entity);

        var runtime = new TotvsRmRuntimeConfiguration(
            true,
            Normalize(request.Server),
            request.Port is > 0 and <= 65535 ? request.Port : entity.Port,
            Normalize(request.Database),
            Normalize(request.UserName),
            password,
            request.TrustServerCertificate,
            request.CodColigada > 0 ? request.CodColigada : entity.CodColigada,
            new TotvsRmModuleFlags(
                request.EnableCadastro,
                request.EnableHolerite,
                request.EnableFerias,
                request.EnableBeneficios,
                request.EnablePonto,
                request.EnableTeamDashboard));

        return await _connectionTester.TestAsync(runtime, cancellationToken);
    }

    public async Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken)
    {
        await EnsureAndGetEntityAsync(cancellationToken);
    }

    private async Task<TotvsRmConfiguration> EnsureAndGetEntityAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TotvsRmConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (entity is not null)
        {
            return entity;
        }

        entity = new TotvsRmConfiguration
        {
            Id = Guid.NewGuid(),
            IsEnabled = false,
            Server = string.Empty,
            Port = 1433,
            Database = string.Empty,
            UserName = string.Empty,
            PasswordProtected = null,
            TrustServerCertificate = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.TotvsRmConfigurations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static TotvsRmConfigurationDto MapToDto(TotvsRmConfiguration entity)
    {
        return new TotvsRmConfigurationDto(
            entity.Id,
            entity.IsEnabled,
            entity.Server,
            entity.Port,
            entity.Database,
            entity.UserName,
            !string.IsNullOrWhiteSpace(entity.PasswordProtected),
            entity.TrustServerCertificate,
            entity.CodColigada > 0 ? entity.CodColigada : (short)1,
            entity.EnableCadastro,
            entity.EnableHolerite,
            entity.EnableFerias,
            entity.EnableBeneficios,
            entity.EnablePonto,
            entity.EnableTeamDashboard,
            entity.UpdatedAtUtc);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string ProtectSecret(string value)
    {
        var plainBytes = Encoding.UTF8.GetBytes(value);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::TotvsRmConfiguration::Secret::v1"));
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var output = new MemoryStream();

        output.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
        }

        return Convert.ToBase64String(output.ToArray());
    }

    private string? TryUnprotectSecret(TotvsRmConfiguration entity)
    {
        if (string.IsNullOrWhiteSpace(entity.PasswordProtected))
        {
            return null;
        }

        try
        {
            return UnprotectSecret(entity.PasswordProtected);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao descriptografar senha TOTVS RM. A integracao sera tratada como indisponivel.");
            return null;
        }
    }

    private static string? UnprotectSecret(string protectedValue)
    {
        var protectedBytes = Convert.FromBase64String(protectedValue);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::TotvsRmConfiguration::Secret::v1"));

        var ivLength = aes.BlockSize / 8;
        var iv = protectedBytes.Take(ivLength).ToArray();
        var cipherBytes = protectedBytes.Skip(ivLength).ToArray();
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var input = new MemoryStream(cipherBytes);
        using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
