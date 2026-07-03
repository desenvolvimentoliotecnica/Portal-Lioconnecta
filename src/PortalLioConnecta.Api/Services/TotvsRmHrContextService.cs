using Microsoft.Extensions.Caching.Memory;
using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public sealed class TotvsRmHrContextService : ITotvsRmHrContextService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly IPortalUserEmployeeIdResolver _employeeIdResolver;
    private readonly ITotvsRmEmployeeRepository _employeeRepository;
    private readonly IMemoryCache _memoryCache;

    public TotvsRmHrContextService(
        IPortalUserEmployeeIdResolver employeeIdResolver,
        ITotvsRmEmployeeRepository employeeRepository,
        IMemoryCache memoryCache)
    {
        _employeeIdResolver = employeeIdResolver;
        _employeeRepository = employeeRepository;
        _memoryCache = memoryCache;
    }

    public async Task<TotvsRmHrResolution> ResolveAsync(
        PortalUser user,
        bool persistWhenFound,
        CancellationToken cancellationToken)
    {
        var resolution = await _employeeIdResolver.ResolveAsync(user, persistWhenFound, cancellationToken);
        if (string.IsNullOrWhiteSpace(resolution.EmployeeId))
        {
            return TotvsRmHrResolution.MissingEmployeeId(
                PortalUserEmployeeIdResolution.BuildMissingProfileMessage(resolution.MessageLabel));
        }

        var chapa = TotvsRmChapaNormalizer.Normalize(resolution.EmployeeId);
        if (string.IsNullOrWhiteSpace(chapa))
        {
            return TotvsRmHrResolution.MissingEmployeeId(
                PortalUserEmployeeIdResolution.BuildMissingProfileMessage(resolution.EmployeeId));
        }

        var cacheKey = $"rm-hr-context:{user.Id}:{chapa}";
        if (_memoryCache.TryGetValue(cacheKey, out TotvsRmHrContext? cachedContext) && cachedContext is not null)
        {
            return TotvsRmHrResolution.Ok(cachedContext, resolution.Source);
        }

        var context = await _employeeRepository.GetHrContextByChapaAsync(chapa, cancellationToken);
        var hrContext = context ?? new TotvsRmHrContext(chapa, null, null);
        _memoryCache.Set(cacheKey, hrContext, CacheDuration);

        return TotvsRmHrResolution.Ok(hrContext, resolution.Source);
    }
}

public sealed record TotvsRmHrResolution(
    bool IsSuccess,
    TotvsRmHrContext? Context,
    string? Source,
    string AvailabilityStatus,
    string? UserMessage)
{
    public static TotvsRmHrResolution Ok(TotvsRmHrContext context, string? source) =>
        new(true, context, source, "ok", null);

    public static TotvsRmHrResolution MissingEmployeeId(string message) =>
        new(false, null, null, "missing_employee_id", message);

    public static TotvsRmHrResolution Disabled(string message) =>
        new(false, null, null, "rm_disabled", message);

    public static TotvsRmHrResolution Unavailable(string message) =>
        new(false, null, null, "rm_unavailable", message);

    public static TotvsRmHrResolution ModuleDisabled(string message) =>
        new(false, null, null, "module_disabled", message);
}
