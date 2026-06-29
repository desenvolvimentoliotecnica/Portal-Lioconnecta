using Microsoft.EntityFrameworkCore;
using PortalLioConnecta.Api.Contracts.Admin.Ldap;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace PortalLioConnecta.Api.Services;

public class LdapConfigurationService : ILdapConfigurationService
{
    private const string DefaultServer = "dc-virtual-02.liotecnica.com.br";
    private const int DefaultPort = 389;
    private const string DefaultBaseDn = "DC=liotecnica,DC=com,DC=br";
    private const string DefaultUserSearchBase = "OU=Departamentos,DC=liotecnica,DC=com,DC=br";
    private const string DefaultNetbiosDomain = "LIOTECNICA";
    private const string DefaultLoginFormat = "domain-backslash-samaccountname";
    private const string DefaultSearchFilter = "(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))";
    private const string DefaultDisplayNameAttribute = "displayName";

    private readonly PortalLioConnectaDbContext _dbContext;
    public LdapConfigurationService(PortalLioConnectaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LdapConfigurationDto> GetAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<LdapConfigurationDto> SaveAsync(UpsertLdapConfigurationRequest request, CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);

        entity.IsEnabled = request.IsEnabled;
        entity.Server = Normalize(request.Server);
        entity.Port = request.Port;
        entity.UseLdaps = request.UseLdaps;
        entity.UseStartTls = request.UseStartTls;
        entity.IgnoreCertificateValidation = request.IgnoreCertificateValidation;
        entity.BaseDn = Normalize(request.BaseDn);
        entity.UserSearchBase = NormalizeNullable(request.UserSearchBase);
        entity.NetbiosDomain = NormalizeNullable(request.NetbiosDomain);
        entity.LoginFormat = Normalize(request.LoginFormat);
        entity.BindDn = NormalizeNullable(request.BindDn);
        entity.SearchFilter = Normalize(request.SearchFilter);
        entity.DisplayNameAttribute = Normalize(request.DisplayNameAttribute);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ServiceAccountPassword))
        {
            entity.BindPasswordProtected = ProtectSecret(request.ServiceAccountPassword.Trim());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<LdapRuntimeConfiguration> GetRuntimeConfigurationAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureAndGetEntityAsync(cancellationToken);
        return new LdapRuntimeConfiguration(
            entity.IsEnabled,
            entity.Server,
            entity.Port,
            entity.UseLdaps,
            entity.UseStartTls,
            entity.IgnoreCertificateValidation,
            entity.BaseDn,
            entity.UserSearchBase,
            entity.NetbiosDomain,
            entity.LoginFormat,
            entity.BindDn,
            string.IsNullOrWhiteSpace(entity.BindPasswordProtected) ? null : UnprotectSecret(entity.BindPasswordProtected),
            entity.SearchFilter,
            entity.DisplayNameAttribute);
    }

    public async Task EnsureDefaultConfigurationAsync(CancellationToken cancellationToken)
    {
        await EnsureAndGetEntityAsync(cancellationToken);
    }

    private async Task<LdapConfiguration> EnsureAndGetEntityAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.LdapConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (entity is not null)
        {
            return entity;
        }

        entity = new LdapConfiguration
        {
            Id = Guid.NewGuid(),
            IsEnabled = true,
            Server = DefaultServer,
            Port = DefaultPort,
            UseLdaps = false,
            UseStartTls = false,
            IgnoreCertificateValidation = false,
            BaseDn = DefaultBaseDn,
            UserSearchBase = DefaultUserSearchBase,
            NetbiosDomain = DefaultNetbiosDomain,
            LoginFormat = DefaultLoginFormat,
            BindDn = string.Empty,
            BindPasswordProtected = null,
            SearchFilter = DefaultSearchFilter,
            DisplayNameAttribute = DefaultDisplayNameAttribute,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.LdapConfigurations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static LdapConfigurationDto MapToDto(LdapConfiguration entity)
    {
        return new LdapConfigurationDto(
            entity.Id,
            entity.IsEnabled,
            entity.Server,
            entity.Port,
            entity.UseLdaps,
            entity.UseStartTls,
            entity.IgnoreCertificateValidation,
            entity.BaseDn,
            entity.UserSearchBase,
            entity.NetbiosDomain,
            entity.LoginFormat,
            entity.BindDn,
            !string.IsNullOrWhiteSpace(entity.BindPasswordProtected),
            entity.SearchFilter,
            entity.DisplayNameAttribute,
            entity.UpdatedAtUtc);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string ProtectSecret(string value)
    {
        var plainBytes = Encoding.UTF8.GetBytes(value);
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::LdapConfiguration::Secret::v1"));
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
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("PortalLioConnecta.Api::LdapConfiguration::Secret::v1"));

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
