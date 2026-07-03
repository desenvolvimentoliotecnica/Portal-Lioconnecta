using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Services;

public class LdapDirectoryAuthenticator : ILdapDirectoryAuthenticator
{
    public Task<LdapAuthenticatedUser?> AuthenticateAsync(
        LdapRuntimeConfiguration configuration,
        string login,
        string password,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(configuration.BindDn) || string.IsNullOrWhiteSpace(configuration.BindPassword))
            {
                return AuthenticateWithDirectBind(configuration, login, password);
            }

            using var directoryConnection = CreateConnection(configuration);
            directoryConnection.Credential = new NetworkCredential(configuration.BindDn, configuration.BindPassword);
            directoryConnection.Bind();

            var baseDn = string.IsNullOrWhiteSpace(configuration.UserSearchBase)
                ? configuration.BaseDn
                : configuration.UserSearchBase;

            var filter = configuration.SearchFilter.Replace("{0}", EscapeFilterValue(login), StringComparison.Ordinal);
            var request = new SearchRequest(
                baseDn,
                filter,
                SearchScope.Subtree,
                new[]
                {
                    configuration.DisplayNameAttribute,
                    "mail",
                    "userPrincipalName",
                    "sAMAccountName",
                    "department",
                    "title",
                    "distinguishedName",
                    "manager",
                    "employeeId",
                    "employeeNumber"
                });

            var response = (SearchResponse)directoryConnection.SendRequest(request);
            var entry = response.Entries.Count > 0 ? response.Entries[0] : null;
            if (entry is null)
            {
                return (LdapAuthenticatedUser?)null;
            }

            var userPrincipalName = ReadAttribute(entry, "userPrincipalName");
            var samAccountName = ReadAttribute(entry, "sAMAccountName");
            var email = ReadAttribute(entry, "mail");
            var displayName = ReadAttribute(entry, configuration.DisplayNameAttribute) ?? samAccountName ?? userPrincipalName ?? login;
            var distinguishedName = ReadAttribute(entry, "distinguishedName");
            var managerDistinguishedName = ReadAttribute(entry, "manager");
            var managerDisplayName = ResolveManagerDisplayName(directoryConnection, configuration, managerDistinguishedName);
            var bindLogin = BuildBindLogin(configuration, login, samAccountName, userPrincipalName, email);

            if (!string.IsNullOrWhiteSpace(configuration.BindDn) && !string.IsNullOrWhiteSpace(configuration.BindPassword))
            {
                using var authenticationConnection = CreateConnection(configuration);
                authenticationConnection.Credential = BuildCredential(configuration, bindLogin, password);
                authenticationConnection.Bind();
            }

            return new LdapAuthenticatedUser(
                login,
                samAccountName,
                userPrincipalName,
                email,
                displayName,
                ReadAttribute(entry, "department"),
                ReadAttribute(entry, "title"),
                distinguishedName,
                managerDisplayName,
                managerDistinguishedName,
                ReadEmployeeId(entry));
        }, cancellationToken);
    }

    private static LdapAuthenticatedUser? AuthenticateWithDirectBind(
        LdapRuntimeConfiguration configuration,
        string login,
        string password)
    {
        foreach (var bindIdentity in BuildDirectBindIdentities(configuration, login))
        {
            using var connection = CreateConnection(configuration);

            try
            {
                connection.Credential = BuildCredential(configuration, bindIdentity, password);
                connection.Bind();

                try
                {
                    var entry = SearchUserEntry(connection, configuration, login);
                    if (entry is not null)
                    {
                        return CreateAuthenticatedUserFromEntry(connection, configuration, login, bindIdentity, entry);
                    }
                }
                catch (DirectoryOperationException)
                {
                    // Alguns ambientes AD aceitam o bind direto, mas bloqueiam busca sem conta de serviço.
                }

                return CreateDirectBindUser(configuration, login, bindIdentity);
            }
            catch (LdapException exception) when (exception.ErrorCode == 49)
            {
                continue;
            }
        }

        return null;
    }

    private static SearchResultEntry? SearchUserEntry(
        LdapConnection connection,
        LdapRuntimeConfiguration configuration,
        string login)
    {
        var baseDn = string.IsNullOrWhiteSpace(configuration.UserSearchBase)
            ? configuration.BaseDn
            : configuration.UserSearchBase;

        var filter = configuration.SearchFilter.Replace("{0}", EscapeFilterValue(login), StringComparison.Ordinal);
        var request = new SearchRequest(
            baseDn,
            filter,
            SearchScope.Subtree,
            new[]
            {
                configuration.DisplayNameAttribute,
                "mail",
                "userPrincipalName",
                "sAMAccountName",
                "department",
                "title",
                "distinguishedName",
                "manager",
                "employeeId",
                "employeeNumber"
            });

        var response = (SearchResponse)connection.SendRequest(request);
        return response.Entries.Count > 0 ? response.Entries[0] : null;
    }

    private static LdapAuthenticatedUser CreateAuthenticatedUserFromEntry(
        LdapConnection connection,
        LdapRuntimeConfiguration configuration,
        string login,
        string bindIdentity,
        SearchResultEntry entry)
    {
        var userPrincipalName = ReadAttribute(entry, "userPrincipalName");
        var samAccountName = ReadAttribute(entry, "sAMAccountName");
        var email = ReadAttribute(entry, "mail");
        var displayName = ReadAttribute(entry, configuration.DisplayNameAttribute) ?? samAccountName ?? userPrincipalName ?? login;
        var distinguishedName = ReadAttribute(entry, "distinguishedName") ?? bindIdentity;
        var managerDistinguishedName = ReadAttribute(entry, "manager");
        var managerDisplayName = ResolveManagerDisplayName(connection, configuration, managerDistinguishedName);

        return new LdapAuthenticatedUser(
            login,
            samAccountName,
            userPrincipalName,
            email,
            displayName,
            ReadAttribute(entry, "department"),
            ReadAttribute(entry, "title"),
            distinguishedName,
            managerDisplayName,
            managerDistinguishedName,
            ReadEmployeeId(entry));
    }

    private static LdapAuthenticatedUser CreateDirectBindUser(
        LdapRuntimeConfiguration configuration,
        string login,
        string bindIdentity)
    {
        var normalizedLogin = ExtractPrincipal(login);
        var samAccountName = ExtractSamAccountName(normalizedLogin);
        var email = normalizedLogin.Contains('@', StringComparison.Ordinal)
            ? normalizedLogin
            : null;
        var userPrincipalName = configuration.LoginFormat is "userprincipalname" or "mail"
            ? normalizedLogin
            : email;
        var displayName = BuildDisplayName(normalizedLogin, samAccountName);

        return new LdapAuthenticatedUser(
            login,
            samAccountName,
            userPrincipalName,
            email,
            displayName,
            null,
            null,
            bindIdentity,
            null,
            null,
            null);
    }

    private static string? ResolveManagerDisplayName(
        LdapConnection connection,
        LdapRuntimeConfiguration configuration,
        string? managerDistinguishedName)
    {
        if (string.IsNullOrWhiteSpace(managerDistinguishedName))
        {
            return null;
        }

        try
        {
            var request = new SearchRequest(
                managerDistinguishedName,
                "(objectClass=*)",
                SearchScope.Base,
                new[]
                {
                    configuration.DisplayNameAttribute,
                    "displayName",
                    "cn",
                    "name"
                });

            var response = (SearchResponse)connection.SendRequest(request);
            var entry = response.Entries.Count > 0 ? response.Entries[0] : null;

            if (entry is null)
            {
                return ExtractCommonName(managerDistinguishedName);
            }

            return NormalizePersonDisplayName(
                ReadAttribute(entry, configuration.DisplayNameAttribute)
                ?? ReadAttribute(entry, "displayName")
                ?? ReadAttribute(entry, "cn")
                ?? ReadAttribute(entry, "name")
                ?? ExtractCommonName(managerDistinguishedName));
        }
        catch (DirectoryOperationException)
        {
            return NormalizePersonDisplayName(ExtractCommonName(managerDistinguishedName));
        }
        catch (LdapException)
        {
            return NormalizePersonDisplayName(ExtractCommonName(managerDistinguishedName));
        }
    }

    private static LdapConnection CreateConnection(LdapRuntimeConfiguration configuration)
    {
        var identifier = new LdapDirectoryIdentifier(configuration.Server, configuration.Port);
        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Basic,
            Timeout = TimeSpan.FromSeconds(20)
        };

        connection.SessionOptions.ProtocolVersion = 3;

        if (configuration.UseLdaps)
        {
            connection.SessionOptions.SecureSocketLayer = true;
        }

        if (configuration.UseStartTls)
        {
            connection.SessionOptions.StartTransportLayerSecurity(null);
        }

        if (configuration.IgnoreCertificateValidation)
        {
            connection.SessionOptions.VerifyServerCertificate += (_, _) => true;
        }

        return connection;
    }

    private static NetworkCredential BuildCredential(LdapRuntimeConfiguration configuration, string bindLogin, string password)
    {
        if (bindLogin.Contains('\\', StringComparison.Ordinal))
        {
            var parts = bindLogin.Split('\\', 2);
            return new NetworkCredential(parts[1], password, parts[0]);
        }

        return new NetworkCredential(bindLogin, password);
    }

    private static IEnumerable<string> BuildDirectBindIdentities(
        LdapRuntimeConfiguration configuration,
        string login)
    {
        var identities = new List<string>();
        var normalizedLogin = ExtractPrincipal(login);
        var samAccountName = ExtractSamAccountName(normalizedLogin);

        switch (configuration.LoginFormat)
        {
            case "domain-backslash-samaccountname":
                identities.Add(BuildDomainSamAccountName(configuration.NetbiosDomain, samAccountName));
                break;
            case "userprincipalname":
            case "mail":
                identities.Add(normalizedLogin);
                break;
            default:
                identities.Add(normalizedLogin);
                if (!string.IsNullOrWhiteSpace(configuration.NetbiosDomain))
                {
                    identities.Add(BuildDomainSamAccountName(configuration.NetbiosDomain, samAccountName));
                }

                identities.Add(samAccountName);
                break;
        }

        return identities
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildBindLogin(
        LdapRuntimeConfiguration configuration,
        string login,
        string? samAccountName,
        string? userPrincipalName,
        string? email)
    {
        return configuration.LoginFormat switch
        {
            "domain-backslash-samaccountname" => BuildDomainSamAccountName(configuration.NetbiosDomain, samAccountName ?? login),
            "userprincipalname" => userPrincipalName ?? login,
            "mail" => email ?? login,
            _ => login.Contains('@', StringComparison.Ordinal)
                ? login
                : (samAccountName ?? login)
        };
    }

    private static string BuildDomainSamAccountName(string? netbiosDomain, string login)
    {
        var samAccountName = ExtractSamAccountName(login);
        if (string.IsNullOrWhiteSpace(netbiosDomain))
        {
            return samAccountName;
        }

        return $"{netbiosDomain}\\{samAccountName}";
    }

    private static string ExtractPrincipal(string login)
    {
        return login.Contains('\\', StringComparison.Ordinal)
            ? login.Split('\\', 2)[1]
            : login;
    }

    private static string ExtractSamAccountName(string login)
    {
        return login.Contains('@', StringComparison.Ordinal)
            ? login.Split('@', 2)[0]
            : login;
    }

    private static string BuildDisplayName(string normalizedLogin, string? samAccountName)
    {
        var source = samAccountName ?? normalizedLogin;
        var parts = source
            .Replace(".", " ", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return source;
        }

        return string.Join(
            ' ',
            parts.Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    private static string? NormalizePersonDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        var hasLowercase = trimmed.Any(char.IsLower);
        if (hasLowercase)
        {
            return trimmed;
        }

        var parts = trimmed
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant());

        return string.Join(' ', parts);
    }

    private static string? ReadEmployeeId(SearchResultEntry entry)
    {
        return ReadAttribute(entry, "employeeId") ?? ReadAttribute(entry, "employeeNumber");
    }

    private static string? ReadAttribute(SearchResultEntry entry, string attributeName)
    {
        if (!entry.Attributes.Contains(attributeName))
        {
            return null;
        }

        var values = entry.Attributes[attributeName];
        return values?.Count > 0 ? values[0]?.ToString() : null;
    }

    private static string? ExtractCommonName(string distinguishedName)
    {
        if (string.IsNullOrWhiteSpace(distinguishedName))
        {
            return null;
        }

        const string prefix = "CN=";
        var parts = distinguishedName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var cnPart = parts.FirstOrDefault(item => item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        return cnPart is null
            ? null
            : cnPart[prefix.Length..].Trim();
    }

    private static string EscapeFilterValue(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '\\' => "\\5c",
                '*' => "\\2a",
                '(' => "\\28",
                ')' => "\\29",
                '\0' => "\\00",
                _ => ch.ToString()
            });
        }

        return builder.ToString();
    }
}
