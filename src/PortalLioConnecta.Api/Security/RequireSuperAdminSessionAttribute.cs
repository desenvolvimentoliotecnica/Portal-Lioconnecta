using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSuperAdminSessionAttribute : Attribute, IAsyncActionFilter
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
        var session = await adminAuthService.GetActiveSessionAsync(token, context.HttpContext.RequestAborted);

        if (session is null)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Sessao administrativa invalida ou expirada."
            });
            return;
        }

        if (!string.Equals(session.User.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                message = "Acesso restrito ao super-admin."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        AdminSessionHttpContext.Store(context.HttpContext, session);
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
