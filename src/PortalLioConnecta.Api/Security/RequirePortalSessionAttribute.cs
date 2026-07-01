using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePortalSessionAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var token = TryReadToken(context.HttpContext.Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Sessao do portal nao informada."
            });
            return;
        }

        var portalAuthService = context.HttpContext.RequestServices.GetRequiredService<IPortalAuthService>();
        var session = await portalAuthService.GetActiveSessionEntityAsync(token, context.HttpContext.RequestAborted);

        if (session?.PortalUser is null || !session.PortalUser.IsActive)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Sessao do portal invalida ou expirada."
            });
            return;
        }

        PortalSessionHttpContext.Store(context.HttpContext, session);
        await next();
    }

    private static string? TryReadToken(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Portal-Token", out var portalToken))
        {
            return portalToken.ToString();
        }

        if (!request.Headers.TryGetValue("Authorization", out var authorization))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        var rawValue = authorization.ToString();
        return rawValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? rawValue[bearerPrefix.Length..].Trim()
            : null;
    }
}
