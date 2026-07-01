using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalLioConnecta.Api.Contracts.Admin.Auth;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class AdminAuthService : IAdminAuthService
{
    public const string DefaultSuperAdminUsername = "super-admin";
    public const string DefaultSuperAdminPassword = "Liotec@2026";

    private static readonly TimeSpan SessionDuration = TimeSpan.FromHours(8);

    private readonly PortalLioConnectaDbContext _dbContext;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;

    public AdminAuthService(
        PortalLioConnectaDbContext dbContext,
        IPasswordHasher<AdminUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedUsername = NormalizeUsername(request.Username);
        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _dbContext.AdminUsers
            .FirstOrDefaultAsync(item => item.Username == normalizedUsername && item.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var session = new AdminSession
        {
            Id = Guid.NewGuid(),
            AdminUserId = user.Id,
            Token = GenerateToken(),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(SessionDuration)
        };

        user.LastLoginAtUtc = now;
        user.UpdatedAtUtc = now;

        _dbContext.AdminSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminLoginResponse(
            session.Token,
            session.ExpiresAtUtc,
            MapProfile(user));
    }

    public async Task<AdminSessionDto?> GetActiveSessionAsync(string token, CancellationToken cancellationToken)
    {
        var normalizedToken = NormalizeToken(token);
        if (string.IsNullOrWhiteSpace(normalizedToken))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var session = await _dbContext.AdminSessions
            .AsNoTracking()
            .Include(item => item.AdminUser)
            .FirstOrDefaultAsync(item =>
                item.Token == normalizedToken &&
                item.RevokedAtUtc == null &&
                item.ExpiresAtUtc > now,
                cancellationToken);

        if (session?.AdminUser is null || !session.AdminUser.IsActive)
        {
            return null;
        }

        return session is null
            ? null
            : new AdminSessionDto(session.Token, session.ExpiresAtUtc, MapProfile(session.AdminUser));
    }

    public async Task<bool> LogoutAsync(string token, CancellationToken cancellationToken)
    {
        var normalizedToken = NormalizeToken(token);
        if (string.IsNullOrWhiteSpace(normalizedToken))
        {
            return false;
        }

        var session = await _dbContext.AdminSessions
            .FirstOrDefaultAsync(item => item.Token == normalizedToken && item.RevokedAtUtc == null, cancellationToken);

        if (session is null)
        {
            return false;
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasActiveSessionAsync(string token, CancellationToken cancellationToken)
    {
        var session = await GetActiveSessionAsync(token, cancellationToken);
        return session is not null;
    }

    public async Task EnsureDefaultSuperAdminAsync(CancellationToken cancellationToken)
    {
        var username = DefaultSuperAdminUsername;
        var existing = await _dbContext.AdminUsers.FirstOrDefaultAsync(item => item.Username == username, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            DisplayName = "Super Admin",
            Role = "SuperAdmin",
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, DefaultSuperAdminPassword);
        _dbContext.AdminUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static AdminProfileDto MapProfile(AdminUser user)
    {
        return new AdminProfileDto(user.Id, user.Username, user.DisplayName, user.Role);
    }

    private static string NormalizeUsername(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string NormalizeToken(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);
    }
}
