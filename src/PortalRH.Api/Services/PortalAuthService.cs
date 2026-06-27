using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalRH.Api.Contracts.Admin.PortalUsers;
using PortalRH.Api.Contracts.Auth;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class PortalAuthService : IPortalAuthService
{
    private static readonly TimeSpan SessionDuration = TimeSpan.FromHours(8);

    private readonly PortalRhDbContext _dbContext;
    private readonly ILdapConfigurationService _ldapConfigurationService;
    private readonly ILdapDirectoryAuthenticator _ldapDirectoryAuthenticator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasswordHasher<PortalUser> _passwordHasher;
    private readonly ILogger<PortalAuthService> _logger;

    public PortalAuthService(
        PortalRhDbContext dbContext,
        ILdapConfigurationService ldapConfigurationService,
        ILdapDirectoryAuthenticator ldapDirectoryAuthenticator,
        IHttpContextAccessor httpContextAccessor,
        IPasswordHasher<PortalUser> passwordHasher,
        ILogger<PortalAuthService> logger)
    {
        _dbContext = dbContext;
        _ldapConfigurationService = ldapConfigurationService;
        _ldapDirectoryAuthenticator = ldapDirectoryAuthenticator;
        _httpContextAccessor = httpContextAccessor;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<PortalLoginResponse?> LoginWithLdapAsync(LdapLoginRequest request, CancellationToken cancellationToken)
    {
        var login = Normalize(request.Login);
        var authContext = ResolveAuthContext();
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(request.Password))
        {
            await RegisterFailedAuthenticationAsync(
                portalUser: null,
                login,
                "Login ou senha ausente.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        var portalUserWithLocalPassword = await FindPortalUserByLoginAsync(login, cancellationToken);
        if (portalUserWithLocalPassword?.PasswordHash is not null)
        {
            return await LoginWithLocalPasswordAsync(
                portalUserWithLocalPassword,
                login,
                request.Password,
                authContext,
                now,
                cancellationToken);
        }

        var ldapConfiguration = await _ldapConfigurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!ldapConfiguration.IsEnabled || string.IsNullOrWhiteSpace(ldapConfiguration.Server))
        {
            await RegisterFailedAuthenticationAsync(
                portalUser: null,
                login,
                "LDAP desabilitado ou sem configuracao ativa.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        LdapAuthenticatedUser? authenticatedUser;
        try
        {
            authenticatedUser = await _ldapDirectoryAuthenticator.AuthenticateAsync(
                ldapConfiguration,
                login,
                request.Password,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao autenticar usuario no LDAP. Login: {Login}", login);
            await RegisterFailedAuthenticationAsync(
                portalUser: null,
                login,
                "Erro tecnico durante a autenticacao LDAP.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        var portalUser = await FindPortalUserByLoginAsync(login, cancellationToken);

        if (authenticatedUser is null)
        {
            await RegisterFailedAuthenticationAsync(
                portalUser,
                login,
                "Credenciais invalidas.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        if (portalUser is null)
        {
            portalUser = new PortalUser
            {
                Id = Guid.NewGuid(),
                Login = authenticatedUser.Login,
                CreatedAtUtc = now,
                Role = PortalUserRoleCatalog.Collaborator,
                ModulePermissionsJson = PortalModulePermissionCatalog.Serialize(
                    PortalModulePermissionCatalog.GetDefaultAssignments(PortalUserRoleCatalog.Collaborator),
                    PortalUserRoleCatalog.Collaborator),
                IsActive = true
            };

            _dbContext.PortalUsers.Add(portalUser);
        }
        else if (!portalUser.IsActive)
        {
            _logger.LogInformation("Login LDAP bloqueado para usuario inativo no portal. Login: {Login}", login);
            await RegisterFailedAuthenticationAsync(
                portalUser,
                login,
                "Usuario bloqueado no portal.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        portalUser.SamAccountName = authenticatedUser.SamAccountName;
        portalUser.UserPrincipalName = authenticatedUser.UserPrincipalName;
        portalUser.Email = authenticatedUser.Email;
        portalUser.DisplayName = authenticatedUser.DisplayName;
        portalUser.Department = authenticatedUser.Department;
        portalUser.Title = authenticatedUser.Title;
        portalUser.DistinguishedName = authenticatedUser.DistinguishedName;
        portalUser.ManagerDisplayName = authenticatedUser.ManagerDisplayName;
        portalUser.ManagerDistinguishedName = authenticatedUser.ManagerDistinguishedName;
        portalUser.LastLoginAtUtc = now;
        portalUser.LastKnownIpAddress = authContext.IpAddress;
        portalUser.LastOrigin = authContext.Origin;
        portalUser.LastFailedLoginAtUtc = null;
        portalUser.FailedLoginCount = 0;
        portalUser.UpdatedAtUtc = now;
        portalUser.Role = string.IsNullOrWhiteSpace(portalUser.Role)
            ? PortalUserRoleCatalog.Collaborator
            : PortalUserRoleCatalog.Normalize(portalUser.Role);

        var currentAssignments = PortalModulePermissionCatalog.DeserializeOrDefault(portalUser.ModulePermissionsJson, portalUser.Role);
        portalUser.ModulePermissionsJson = PortalModulePermissionCatalog.Serialize(currentAssignments, portalUser.Role);

        return await CompleteLoginAsync(portalUser, "LDAP", authContext, now, cancellationToken);
    }

    private async Task<PortalLoginResponse?> LoginWithLocalPasswordAsync(
        PortalUser portalUser,
        string login,
        string password,
        PortalRequestAuditContext authContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var verification = _passwordHasher.VerifyHashedPassword(portalUser, portalUser.PasswordHash!, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            await RegisterFailedAuthenticationAsync(
                portalUser,
                login,
                "Credenciais invalidas.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        if (!portalUser.IsActive)
        {
            await RegisterFailedAuthenticationAsync(
                portalUser,
                login,
                "Usuario bloqueado no portal.",
                authContext,
                now,
                cancellationToken);
            return null;
        }

        portalUser.LastLoginAtUtc = now;
        portalUser.LastKnownIpAddress = authContext.IpAddress;
        portalUser.LastOrigin = authContext.Origin;
        portalUser.LastFailedLoginAtUtc = null;
        portalUser.FailedLoginCount = 0;
        portalUser.UpdatedAtUtc = now;
        portalUser.Role = PortalUserRoleCatalog.Normalize(portalUser.Role);

        var currentAssignments = PortalModulePermissionCatalog.DeserializeOrDefault(portalUser.ModulePermissionsJson, portalUser.Role);
        portalUser.ModulePermissionsJson = PortalModulePermissionCatalog.Serialize(currentAssignments, portalUser.Role);

        return await CompleteLoginAsync(portalUser, "Local", authContext, now, cancellationToken);
    }

    private async Task<PortalLoginResponse> CompleteLoginAsync(
        PortalUser portalUser,
        string authenticationProvider,
        PortalRequestAuditContext authContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var session = new PortalSession
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUser.Id,
            Token = GenerateToken(),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(SessionDuration)
        };

        _dbContext.PortalSessions.Add(session);
        _dbContext.PortalUserLoginEvents.Add(new PortalUserLoginEvent
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUser.Id,
            Login = portalUser.Login,
            DisplayNameSnapshot = portalUser.DisplayName,
            EmailSnapshot = portalUser.Email,
            DepartmentSnapshot = portalUser.Department,
            AuthenticationProvider = authenticationProvider,
            EventType = PortalAuthEventTypes.LoginSuccess,
            IsSuccess = true,
            IpAddress = authContext.IpAddress,
            Origin = authContext.Origin,
            UserAgent = authContext.UserAgent,
            LoggedAtUtc = now
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PortalLoginResponse(
            session.Token,
            session.ExpiresAtUtc,
            MapProfile(portalUser));
    }

    private Task<PortalUser?> FindPortalUserByLoginAsync(string login, CancellationToken cancellationToken)
    {
        return _dbContext.PortalUsers
            .FirstOrDefaultAsync(item =>
                item.Login == login ||
                (item.UserPrincipalName != null && item.UserPrincipalName == login) ||
                (item.Email != null && item.Email == login) ||
                (item.SamAccountName != null && item.SamAccountName == login),
                cancellationToken);
    }

    public async Task<PortalLoginResponse?> GetSessionAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var session = await GetActiveSessionAsync(token, cancellationToken);
        if (session?.PortalUser is null || !session.PortalUser.IsActive)
        {
            return null;
        }

        return new PortalLoginResponse(
            session.Token,
            session.ExpiresAtUtc,
            MapProfile(session.PortalUser));
    }

    public Task<PortalSession?> GetActiveSessionEntityAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<PortalSession?>(null);
        }

        return GetActiveSessionAsync(token, cancellationToken);
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var session = await _dbContext.PortalSessions
            .Include(item => item.PortalUser)
            .FirstOrDefaultAsync(item => item.Token == token, cancellationToken);

        if (session is null || session.RevokedAtUtc is not null)
        {
            return;
        }

        var authContext = ResolveAuthContext();
        session.RevokedAtUtc = DateTime.UtcNow;

        _dbContext.PortalUserLoginEvents.Add(new PortalUserLoginEvent
        {
            Id = Guid.NewGuid(),
            PortalUserId = session.PortalUserId,
            Login = session.PortalUser.Login,
            DisplayNameSnapshot = session.PortalUser.DisplayName,
            EmailSnapshot = session.PortalUser.Email,
            DepartmentSnapshot = session.PortalUser.Department,
            AuthenticationProvider = "LDAP",
            EventType = PortalAuthEventTypes.Logout,
            IsSuccess = true,
            IpAddress = authContext.IpAddress,
            Origin = authContext.Origin,
            UserAgent = authContext.UserAgent,
            LoggedAtUtc = session.RevokedAtUtc.Value
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private async Task<PortalSession?> GetActiveSessionAsync(string token, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.PortalSessions
            .Include(item => item.PortalUser)
            .FirstOrDefaultAsync(
                item => item.Token == token &&
                    item.RevokedAtUtc == null &&
                    item.ExpiresAtUtc > now,
                cancellationToken);
    }

    private PortalUserProfileDto MapProfile(PortalUser portalUser)
    {
        var normalizedRole = PortalUserRoleCatalog.Normalize(portalUser.Role);
        var modulePermissions = PortalModulePermissionCatalog.DeserializeOrDefault(portalUser.ModulePermissionsJson, normalizedRole);

        return new PortalUserProfileDto(
            portalUser.Id,
            portalUser.Login,
            portalUser.DisplayName,
            portalUser.Email,
            portalUser.Department,
            portalUser.Title,
            portalUser.ManagerDisplayName,
            normalizedRole,
            PortalUserRoleCatalog.GetLabel(normalizedRole),
            PortalUserRoleCatalog.GetPermissions(normalizedRole),
            modulePermissions.Select(permission => new PortalUserModulePermissionDto(
                permission.ModuleKey,
                PortalModulePermissionCatalog.GetModuleLabel(permission.ModuleKey),
                permission.AccessLevel,
                PortalModulePermissionCatalog.GetAccessLevelLabel(permission.AccessLevel)))
            .ToList());
    }

    private async Task RegisterFailedAuthenticationAsync(
        PortalUser? portalUser,
        string login,
        string failureReason,
        PortalRequestAuditContext authContext,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        if (portalUser is not null)
        {
            portalUser.FailedLoginCount += 1;
            portalUser.LastFailedLoginAtUtc = occurredAtUtc;
            portalUser.LastKnownIpAddress = authContext.IpAddress;
            portalUser.LastOrigin = authContext.Origin;
            portalUser.UpdatedAtUtc = occurredAtUtc;
        }

        _dbContext.PortalUserLoginEvents.Add(new PortalUserLoginEvent
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUser?.Id,
            Login = login,
            DisplayNameSnapshot = portalUser?.DisplayName ?? login,
            EmailSnapshot = portalUser?.Email,
            DepartmentSnapshot = portalUser?.Department,
            AuthenticationProvider = "LDAP",
            EventType = PortalAuthEventTypes.LoginFailure,
            IsSuccess = false,
            FailureReason = failureReason,
            IpAddress = authContext.IpAddress,
            Origin = authContext.Origin,
            UserAgent = authContext.UserAgent,
            LoggedAtUtc = occurredAtUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private PortalRequestAuditContext ResolveAuthContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return new PortalRequestAuditContext(null, null, null);
        }

        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        }

        if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Contains(','))
        {
            ipAddress = ipAddress.Split(',')[0].Trim();
        }

        var origin = httpContext.Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(origin) && httpContext.Request.Host.HasValue)
        {
            origin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        }

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        return new PortalRequestAuditContext(ipAddress, origin, userAgent);
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);
    }

    private sealed record PortalRequestAuditContext(
        string? IpAddress,
        string? Origin,
        string? UserAgent);
}
