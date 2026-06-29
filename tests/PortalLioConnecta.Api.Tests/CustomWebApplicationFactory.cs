using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PortalLioConnectaDbContext>>();
            services.RemoveAll<PortalLioConnectaDbContext>();
            services.RemoveAll<ILdapDirectoryAuthenticator>();

            services.AddDbContext<PortalLioConnectaDbContext>(options =>
                options.UseInMemoryDatabase("portalrh-api-tests", DatabaseRoot));

            services.AddSingleton<ILdapDirectoryAuthenticator, FakeLdapDirectoryAuthenticator>();
        });
    }
}

public class FakeLdapDirectoryAuthenticator : ILdapDirectoryAuthenticator
{
    public Task<LdapAuthenticatedUser?> AuthenticateAsync(
        LdapRuntimeConfiguration configuration,
        string login,
        string password,
        CancellationToken cancellationToken)
    {
        if (!configuration.IsEnabled || string.IsNullOrWhiteSpace(configuration.Server))
        {
            return Task.FromResult<LdapAuthenticatedUser?>(null);
        }

        if (!string.Equals(login, "roberto.almeida@liotecnica.com.br", StringComparison.OrdinalIgnoreCase) ||
            password != "Liotec@2026")
        {
            return Task.FromResult<LdapAuthenticatedUser?>(null);
        }

        return Task.FromResult<LdapAuthenticatedUser?>(new LdapAuthenticatedUser(
            "roberto.almeida@liotecnica.com.br",
            "roberto.almeida",
            "roberto.almeida@liotecnica.com.br",
            "roberto.almeida@liotecnica.com.br",
            "Roberto Almeida",
            "Recursos Humanos",
            "Analista de RH",
            "CN=Roberto Almeida,OU=Usuarios,DC=liotecnica,DC=com,DC=br",
            "Elizabete Rodrigues da Silva",
            "CN=Elizabete Rodrigues da Silva,OU=Gestores,DC=liotecnica,DC=com,DC=br"));
    }
}
