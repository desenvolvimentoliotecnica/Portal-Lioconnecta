using PortalLioConnecta.Api.Contracts.Admin.Auth;

namespace PortalLioConnecta.Api.Security;

public static class AdminSessionHttpContext
{
    public const string SessionItemKey = "PortalLioConnecta.AdminSession";

    public static void Store(HttpContext context, AdminSessionDto session)
    {
        context.Items[SessionItemKey] = session;
    }

    public static AdminSessionDto? Get(HttpContext context)
    {
        return context.Items.TryGetValue(SessionItemKey, out var value)
            ? value as AdminSessionDto
            : null;
    }
}
