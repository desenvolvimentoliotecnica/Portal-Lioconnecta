using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Data;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace PortalRH.Api.Services;

public class MicrosoftGraphConfigurationService : IMicrosoftGraphConfigurationService
{
    private const string DefaultUserIdentifier = "userPrincipalName";
    private static readonly HashSet<string> AllowedUserIdentifiers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "userPrincipalName",
            "mail"
        };

    private readonly PortalRhDbContext _dbContext;
    private readonly MicrosoftGraphConnectionTester _connectionTester;

    public MicrosoftGraphConfigurationService(
        PortalRhDbContext dbContext,
        MicrosoftGraphConnectionTester connectionTester)
    {
        _dbContext = dbContext;
        _connectionTester = connectionTester;
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
            string.IsNullOrWhiteSpace(entity.ClientSecretProtected) ? null : UnprotectSecret(entity.ClientSecretProtected),
            entity.UserIdentifier);
    }

    public async Task<MicrosoftGraphConnectionTestResponse> TestConnectionAsync(
        UpsertMicrosoftGraphConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        var clientSecret = !string.IsNullOrWhiteSpace(request.ClientSecret)
            ? request.ClientSecret.Trim()
            : string.IsNullOrWhiteSpace(entity.ClientSecretProtected)
                ? null
                : UnprotectSecret(entity.ClientSecretProtected);

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
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalRH.Api::MicrosoftGraphConfiguration::Secret::v1"));
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

    private static string? UnprotectSecret(string? protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            return null;
        }

        var protectedBytes = Convert.FromBase64String(protectedValue);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalRH.Api::MicrosoftGraphConfiguration::Secret::v1"));

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
