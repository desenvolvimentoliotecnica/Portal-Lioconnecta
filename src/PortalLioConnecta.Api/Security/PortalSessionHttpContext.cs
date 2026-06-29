using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Security;

public static class PortalSessionHttpContext
{
    public const string SessionItemKey = "PortalLioConnecta.PortalSession";

    public static void Store(HttpContext context, PortalSession session)
    {
        context.Items[SessionItemKey] = session;
    }

    public static PortalSession? Get(HttpContext context)
    {
        return context.Items.TryGetValue(SessionItemKey, out var value)
            ? value as PortalSession
            : null;
    }
}
