using Microsoft.EntityFrameworkCore;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Services;

public static class PortalLioConnectaDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PortalLioConnectaDbContext>();
        var adminAuthService = scope.ServiceProvider.GetRequiredService<IAdminAuthService>();
        var ldapConfigurationService = scope.ServiceProvider.GetRequiredService<ILdapConfigurationService>();
        var microsoftGraphConfigurationService = scope.ServiceProvider.GetRequiredService<IMicrosoftGraphConfigurationService>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await adminAuthService.EnsureDefaultSuperAdminAsync(cancellationToken);
        await ldapConfigurationService.EnsureDefaultConfigurationAsync(cancellationToken);
        await microsoftGraphConfigurationService.EnsureDefaultConfigurationAsync(cancellationToken);

        var moodSurveyFeedbackService = scope.ServiceProvider.GetRequiredService<IMoodSurveyFeedbackService>();
        await moodSurveyFeedbackService.EnsureSeedAsync(cancellationToken);

        var quickLinkService = scope.ServiceProvider.GetRequiredService<IQuickLinkService>();
        await quickLinkService.EnsureSeedAsync(cancellationToken);

        var portalUserSeedService = scope.ServiceProvider.GetRequiredService<IPortalUserSeedService>();
        await portalUserSeedService.EnsureSeedAsync(cancellationToken);
    }
}
