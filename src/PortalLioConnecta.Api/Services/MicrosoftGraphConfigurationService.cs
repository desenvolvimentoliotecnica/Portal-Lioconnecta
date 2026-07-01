using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace PortalLioConnecta.Api.Services;

public class MicrosoftGraphConfigurationService : IMicrosoftGraphConfigurationService
{
    private const string DefaultUserIdentifier = "userPrincipalName";
    private static readonly HashSet<string> AllowedUserIdentifiers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "userPrincipalName",
            "mail"
        };

    private readonly PortalLioConnectaDbContext _dbContext;
    private readonly MicrosoftGraphConnectionTester _connectionTester;
    private readonly ILogger<MicrosoftGraphConfigurationService> _logger;

    public MicrosoftGraphConfigurationService(
        PortalLioConnectaDbContext dbContext,
        MicrosoftGraphConnectionTester connectionTester,
        ILogger<MicrosoftGraphConfigurationService> logger)
    {
        _dbContext = dbContext;
        _connectionTester = connectionTester;
        _logger = logger;
    }

    public async Task<MicrosoftGraphConfigurationDto> GetAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<MicrosoftGraphConfigurationDto> SaveAsync(UpsertMicrosoftGraphConfigurationRequest request, CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);

        entity.IsEnabled = request.IsEnabled;
        entity.TenantId = Normalize(request.TenantId);
        entity.ClientId = Normalize(request.ClientId);
        entity.UserIdentifier = NormalizeUserIdentifier(request.UserIdentifier);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            entity.ClientSecretProtected = ProtectSecret(request.ClientSecret.Trim());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<MicrosoftGraphRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return new MicrosoftGraphRuntimeConfiguration(
            entity.IsEnabled,
            entity.TenantId,
            entity.ClientId,
            TryUnprotectSecret(entity),
            entity.UserIdentifier);
    }

    public async Task<MicrosoftGraphConnectionTestResponse> TestConnectionAsync(
        UpsertMicrosoftGraphConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        var clientSecret = !string.IsNullOrWhiteSpace(request.ClientSecret)
            ? request.ClientSecret.Trim()
            : TryUnprotectSecret(entity);

        return await _connectionTester.TestAsync(
            Normalize(request.TenantId),
            Normalize(request.ClientId),
            clientSecret ?? string.Empty,
            cancellationToken);
    }

    public async Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken)
    {
        await EnsureAndGetEntityAsync(cancellationToken);
    }

    private async Task<MicrosoftGraphConfiguration> EnsureAndGetEntityAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MicrosoftGraphConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (entity is not null)
        {
            return entity;
        }

        entity = new MicrosoftGraphConfiguration
        {
            Id = Guid.NewGuid(),
            IsEnabled = false,
            TenantId = string.Empty,
            ClientId = string.Empty,
            ClientSecretProtected = null,
            UserIdentifier = DefaultUserIdentifier,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.MicrosoftGraphConfigurations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static MicrosoftGraphConfigurationDto MapToDto(MicrosoftGraphConfiguration entity)
    {
        return new MicrosoftGraphConfigurationDto(
            entity.Id,
            entity.IsEnabled,
            entity.TenantId,
            entity.ClientId,
            !string.IsNullOrWhiteSpace(entity.ClientSecretProtected),
            entity.UserIdentifier,
            entity.UpdatedAtUtc);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string NormalizeUserIdentifier(string? value)
    {
        var normalized = Normalize(value);
        return AllowedUserIdentifiers.Contains(normalized)
            ? normalized
            : DefaultUserIdentifier;
    }

    private static string ProtectSecret(string value)
    {
        var plainBytes = Encoding.UTF8.GetBytes(value);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::MicrosoftGraphConfiguration::Secret::v1"));
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

    private string? TryUnprotectSecret(MicrosoftGraphConfiguration entity)
    {
        if (!entity.IsEnabled || string.IsNullOrWhiteSpace(entity.ClientSecretProtected))
        {
            return null;
        }

        try
        {
            return UnprotectSecret(entity.ClientSecretProtected);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao descriptografar segredo do Microsoft Graph. A integracao sera tratada como indisponivel.");
            return null;
        }
    }

    private static string? UnprotectSecret(string protectedValue)
    {
        var protectedBytes = Convert.FromBase64String(protectedValue);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::MicrosoftGraphConfiguration::Secret::v1"));

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
