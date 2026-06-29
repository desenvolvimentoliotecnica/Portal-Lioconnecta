using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAdminSessionAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var token = TryReadToken(context.HttpContext.Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Sessao administrativa nao informada."
            });
            return;
        }

        var adminAuthService = context.HttpContext.RequestServices.GetRequiredService<IAdminAuthService>();
        var isValid = await adminAuthService.HasActiveSessionAsync(token, context.HttpContext.RequestAborted);

        if (!isValid)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Sessao administrativa invalida ou expirada."
            });
            return;
        }

        await next();
    }

    private static string? TryReadToken(HttpRequest request)
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
}
