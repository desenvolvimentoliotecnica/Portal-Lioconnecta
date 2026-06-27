using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalRH.Api.Interfaces;

namespace PortalRH.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireCommunicationEditorAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeAdminSessionAsync(context))
        {
            await next();
            return;
        }

        if (await TryAuthorizePortalEditorAsync(context))
        {
            await next();
            return;
        }

        context.Result = new UnauthorizedObjectResult(new
        {
            message = "Sessao invalida ou sem permissao para gerenciar comunicados."
        });
    }

    private static async Task<bool> TryAuthorizeAdminSessionAsync(ActionExecutingContext context)
    {
        var token = TryReadAdminToken(context.HttpContext.Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var adminAuthService = context.HttpContext.RequestServices.GetRequiredService<IAdminAuthService>();
        return await adminAuthService.HasActiveSessionAsync(token, context.HttpContext.RequestAborted);
    }

    private static async Task<bool> TryAuthorizePortalEditorAsync(ActionExecutingContext context)
    {
        var token = TryReadPortalToken(context.HttpContext.Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var portalAuthService = context.HttpContext.RequestServices.GetRequiredService<IPortalAuthService>();
        var session = await portalAuthService.GetActiveSessionEntityAsync(token, context.HttpContext.RequestAborted);

        if (session?.PortalUser is null || !session.PortalUser.IsActive)
        {
            return false;
        }

        if (!PortalModuleAccess.CanManageCommunications(session.PortalUser))
        {
            return false;
        }

        PortalSessionHttpContext.Store(context.HttpContext, session);
        return true;
    }

    private static string? TryReadAdminToken(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Admin-Token", out var adminToken))
        {
            return adminToken.ToString();
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

    private static string? TryReadPortalToken(HttpRequest request)
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
