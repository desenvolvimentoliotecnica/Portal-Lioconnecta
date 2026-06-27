using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Admin.Auth;
using PortalRH.Api.Contracts.Admin.PortalUsers;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class PortalUserAdminService : IPortalUserAdminService
{
    private readonly PortalRhDbContext _dbContext;

    public PortalUserAdminService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortalUserAdminListResponse> GetAllAsync(
        string? query,
        string? status,
        string? role,
        string? department,
        string? sortBy,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        var normalizedStatus = NormalizeStatus(status);
        var normalizedRole = NormalizeRoleFilter(role);
        var normalizedDepartment = NormalizeDepartmentFilter(department);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var currentPage = Math.Max(1, page);
        var currentPageSize = Math.Clamp(pageSize, 1, 50);

        var usersQuery = _dbContext.PortalUsers
            .AsNoTracking()
            .Include(item => item.LoginEvents)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var search = normalizedQuery.ToLowerInvariant();
            usersQuery = usersQuery.Where(item =>
                item.DisplayName.ToLower().Contains(search) ||
                item.Login.ToLower().Contains(search) ||
                (item.Email != null && item.Email.ToLower().Contains(search)) ||
                (item.Department != null && item.Department.ToLower().Contains(search)) ||
                (item.Title != null && item.Title.ToLower().Contains(search)) ||
                (item.ManagerDisplayName != null && item.ManagerDisplayName.ToLower().Contains(search)));
        }

        if (normalizedStatus == "active")
        {
            usersQuery = usersQuery.Where(item => item.IsActive);
        }
        else if (normalizedStatus == "inactive")
        {
            usersQuery = usersQuery.Where(item => !item.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(normalizedRole))
        {
            usersQuery = usersQuery.Where(item => item.Role == normalizedRole);
        }

        var departmentOptions = await usersQuery
            .Where(item => item.Department != null && item.Department != string.Empty)
            .GroupBy(item => item.Department!)
            .Select(group => new
            {
                Key = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.Key)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(normalizedDepartment))
        {
            var departmentSearch = normalizedDepartment.ToLowerInvariant();
            usersQuery = usersQuery.Where(item => item.Department != null && item.Department.ToLower() == departmentSearch);
        }

        usersQuery = ApplySorting(usersQuery, normalizedSortBy, normalizedSortDirection);

        var totalItems = await usersQuery.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)currentPageSize));
        currentPage = Math.Min(currentPage, totalPages);

        var items = await usersQuery
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .ToListAsync(cancellationToken);

        var summary = await BuildSummaryAsync(cancellationToken);
        var recentLogins = await _dbContext.PortalUserLoginEvents
            .AsNoTracking()
            .OrderByDescending(item => item.LoggedAtUtc)
            .Take(12)
            .Select(item => new PortalUserLoginEventDto(
                item.Id,
                item.PortalUserId,
                item.Login,
                item.DisplayNameSnapshot,
                item.DepartmentSnapshot,
                item.AuthenticationProvider,
                item.EventType,
                GetAuthEventTypeLabel(item.EventType),
                item.IsSuccess,
                item.FailureReason,
                item.IpAddress,
                item.Origin,
                item.LoggedAtUtc))
            .ToListAsync(cancellationToken);

        var recentAuditEntries = await _dbContext.PortalUserAdminAuditLogs
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(12)
            .Select(item => new PortalUserAdminAuditLogDto(
                item.Id,
                item.PortalUserId,
                item.PortalUser.DisplayName,
                item.ActionType,
                item.ActorUsername,
                item.ActorDisplayName,
                item.ActorRole,
                item.PreviousValue,
                item.NewValue,
                item.Notes,
                item.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var recentMoodSurveyEntries = await _dbContext.MoodSurveyAuditLogs
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(12)
            .ToListAsync(cancellationToken);

        var moodSurveyAuditDtos = recentMoodSurveyEntries
            .Select(item =>
            {
                var option = MoodSurveyOptionCatalog.Find(item.OptionKey);
                return new MoodSurveyAuditLogDto(
                    item.Id,
                    item.PortalUserId,
                    item.ActorDisplayName,
                    item.PortalUser?.Department,
                    item.OptionKey,
                    option?.Label ?? item.OptionKey,
                    option?.Emoji ?? "🙂",
                    item.ActionType,
                    MoodSurveyAuditActionTypes.GetLabel(item.ActionType),
                    item.IpAddress,
                    item.Origin,
                    item.SurveyDate,
                    item.CreatedAtUtc);
            })
            .ToList();

        return new PortalUserAdminListResponse(
            items.Select(MapToDto).ToList(),
            summary,
            PortalUserRoleCatalog.GetAll()
                .Select(item => new PortalUserRoleOptionDto(item.Key, item.Label, item.Permissions))
                .ToList(),
            departmentOptions
                .Select(item => new PortalUserDepartmentOptionDto(item.Key, item.Key, item.Count))
                .ToList(),
            PortalModulePermissionCatalog.GetModules()
                .Select(item => new PortalUserModuleOptionDto(item.Key, item.Label))
                .ToList(),
            PortalModulePermissionCatalog.GetAccessLevels()
                .Select(item => new PortalUserAccessLevelOptionDto(item.Key, item.Label))
                .ToList(),
            recentLogins,
            recentAuditEntries,
            moodSurveyAuditDtos,
            currentPage,
            currentPageSize,
            totalItems,
            totalPages,
            normalizedQuery,
            normalizedStatus,
            normalizedRole,
            normalizedDepartment,
            normalizedSortBy,
            normalizedSortDirection);
    }

    public async Task<PortalUserAdminDto?> UpdateStatusAsync(Guid id, bool isActive, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var user = await _dbContext.PortalUsers
            .Include(item => item.LoginEvents)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var previousValue = user.IsActive ? "Ativo" : "Inativo";
        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        if (!isActive)
        {
            var activeSessions = await _dbContext.PortalSessions
                .Where(item => item.PortalUserId == user.Id && item.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var session in activeSessions)
            {
                session.RevokedAtUtc = DateTime.UtcNow;
            }
        }

        await WriteAuditAsync(
            user,
            actor,
            "StatusAlterado",
            previousValue,
            isActive ? "Ativo" : "Inativo",
            isActive ? "Acesso reativado pelo super-admin." : "Acesso desativado pelo super-admin.",
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task<PortalUserAdminDto?> UpdateRoleAsync(Guid id, string role, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var normalizedRole = PortalUserRoleCatalog.Normalize(role);
        var user = await _dbContext.PortalUsers
            .Include(item => item.LoginEvents)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var previousRole = user.Role;
        user.Role = normalizedRole;
        user.ModulePermissionsJson = PortalModulePermissionCatalog.Serialize(
            PortalModulePermissionCatalog.GetDefaultAssignments(normalizedRole),
            normalizedRole);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await WriteAuditAsync(
            user,
            actor,
            "PerfilAlterado",
            PortalUserRoleCatalog.GetLabel(previousRole),
            PortalUserRoleCatalog.GetLabel(normalizedRole),
            "Perfil do usuario atualizado e permissoes por modulo redefinidas.",
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task<PortalUserAdminDto?> UpdateModulePermissionAsync(Guid id, string moduleKey, string accessLevel, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var user = await _dbContext.PortalUsers
            .Include(item => item.LoginEvents)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var normalizedModuleKey = PortalModulePermissionCatalog.NormalizeModuleKey(moduleKey);
        var normalizedAccessLevel = PortalModulePermissionCatalog.NormalizeAccessLevel(accessLevel);
        var assignments = PortalModulePermissionCatalog.DeserializeOrDefault(user.ModulePermissionsJson, user.Role).ToList();
        var target = assignments.First(item => item.ModuleKey == normalizedModuleKey);
        var previousValue = PortalModulePermissionCatalog.GetAccessLevelLabel(target.AccessLevel);

        assignments.Remove(target);
        assignments.Add(new PortalModulePermissionAssignment(normalizedModuleKey, normalizedAccessLevel));

        user.ModulePermissionsJson = PortalModulePermissionCatalog.Serialize(assignments, user.Role);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await WriteAuditAsync(
            user,
            actor,
            "PermissaoModuloAlterada",
            $"{PortalModulePermissionCatalog.GetModuleLabel(normalizedModuleKey)}: {previousValue}",
            $"{PortalModulePermissionCatalog.GetModuleLabel(normalizedModuleKey)}: {PortalModulePermissionCatalog.GetAccessLevelLabel(normalizedAccessLevel)}",
            "Permissao granular atualizada na gestao de usuarios.",
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    private async Task<PortalUserAdminSummaryDto> BuildSummaryAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.PortalUsers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var authEvents = await _dbContext.PortalUserLoginEvents
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PortalUserAdminSummaryDto(
            users.Count,
            users.Count(item => item.IsActive),
            users.Count(item => !item.IsActive),
            users.Count(item => !string.IsNullOrWhiteSpace(item.Department)),
            users.Count(item => item.Role == PortalUserRoleCatalog.PortalAdmin),
            authEvents.Count(item => item.IsSuccess && item.EventType == PortalAuthEventTypes.LoginSuccess),
            authEvents.Count(item => !item.IsSuccess && item.EventType == PortalAuthEventTypes.LoginFailure),
            authEvents.Count(item => item.EventType == PortalAuthEventTypes.Logout),
            await _dbContext.MoodSurveyAuditLogs.AsNoTracking().CountAsync(cancellationToken));
    }

    private async Task WriteAuditAsync(
        PortalUser portalUser,
        AdminProfileDto actor,
        string actionType,
        string? previousValue,
        string? newValue,
        string? notes,
        CancellationToken cancellationToken)
    {
        var adminUser = await _dbContext.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == actor.Id, cancellationToken);

        _dbContext.PortalUserAdminAuditLogs.Add(new PortalUserAdminAuditLog
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUser.Id,
            AdminUserId = adminUser?.Id,
            ActionType = actionType,
            ActorUsername = actor.Username,
            ActorDisplayName = actor.DisplayName,
            ActorRole = actor.Role,
            PreviousValue = previousValue,
            NewValue = newValue,
            Notes = notes,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static PortalUserAdminDto MapToDto(PortalUser item)
    {
        var normalizedRole = PortalUserRoleCatalog.Normalize(item.Role);
        var modulePermissions = PortalModulePermissionCatalog.DeserializeOrDefault(item.ModulePermissionsJson, normalizedRole);

        return new PortalUserAdminDto(
            item.Id,
            item.Login,
            item.DisplayName,
            item.Email,
            item.Department,
            item.Title,
            item.ManagerDisplayName,
            "LDAP",
            normalizedRole,
            PortalUserRoleCatalog.GetLabel(normalizedRole),
            PortalUserRoleCatalog.GetPermissions(normalizedRole),
            modulePermissions.Select(permission => new PortalUserModulePermissionDto(
                permission.ModuleKey,
                PortalModulePermissionCatalog.GetModuleLabel(permission.ModuleKey),
                permission.AccessLevel,
                PortalModulePermissionCatalog.GetAccessLevelLabel(permission.AccessLevel)))
            .ToList(),
            item.IsActive,
            item.LoginEvents.Count(entry => entry.IsSuccess && entry.EventType == PortalAuthEventTypes.LoginSuccess),
            item.FailedLoginCount,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            item.LastLoginAtUtc,
            item.LastFailedLoginAtUtc,
            item.LastKnownIpAddress,
            item.LastOrigin);
    }

    private static string NormalizeStatus(string? status)
    {
        var value = status?.Trim().ToLowerInvariant();
        return value is "active" or "inactive" ? value : "all";
    }

    private static string NormalizeRoleFilter(string? role)
    {
        return string.IsNullOrWhiteSpace(role) || string.Equals(role, "all", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : PortalUserRoleCatalog.Normalize(role);
    }

    private static string NormalizeDepartmentFilter(string? department)
    {
        return string.IsNullOrWhiteSpace(department) || string.Equals(department, "all", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : department.Trim();
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "displayname" => "displayName",
            "email" => "email",
            "department" => "department",
            "role" => "role",
            "status" => "status",
            "lastlogin" => "lastLogin",
            "failedlogins" => "failedLogins",
            _ => "displayName"
        };
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        return string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";
    }

    private static IQueryable<PortalUser> ApplySorting(IQueryable<PortalUser> query, string sortBy, string sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "email" => isDescending
                ? query.OrderByDescending(item => item.Email).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.Email).ThenBy(item => item.DisplayName),
            "department" => isDescending
                ? query.OrderByDescending(item => item.Department).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.Department).ThenBy(item => item.DisplayName),
            "role" => isDescending
                ? query.OrderByDescending(item => item.Role).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.Role).ThenBy(item => item.DisplayName),
            "status" => isDescending
                ? query.OrderByDescending(item => item.IsActive).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.IsActive).ThenBy(item => item.DisplayName),
            "lastLogin" => isDescending
                ? query.OrderByDescending(item => item.LastLoginAtUtc ?? item.UpdatedAtUtc).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.LastLoginAtUtc ?? item.UpdatedAtUtc).ThenBy(item => item.DisplayName),
            "failedLogins" => isDescending
                ? query.OrderByDescending(item => item.FailedLoginCount).ThenBy(item => item.DisplayName)
                : query.OrderBy(item => item.FailedLoginCount).ThenBy(item => item.DisplayName),
            _ => isDescending
                ? query.OrderByDescending(item => item.DisplayName).ThenBy(item => item.Login)
                : query.OrderBy(item => item.DisplayName).ThenBy(item => item.Login)
        };
    }

    private static string GetAuthEventTypeLabel(string eventType)
    {
        return eventType switch
        {
            PortalAuthEventTypes.LoginSuccess => "Login efetuado",
            PortalAuthEventTypes.LoginFailure => "Tentativa falha",
            PortalAuthEventTypes.Logout => "Logout",
            _ => "Evento de autenticacao"
        };
    }
}
