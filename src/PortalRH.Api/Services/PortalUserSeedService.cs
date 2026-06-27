using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class PortalUserSeedService : IPortalUserSeedService
{
    private readonly PortalRhDbContext _dbContext;
    private readonly IPasswordHasher<PortalUser> _passwordHasher;

    public PortalUserSeedService(
        PortalRhDbContext dbContext,
        IPasswordHasher<PortalUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task EnsureSeedAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var passwordHash = _passwordHasher.HashPassword(new PortalUser(), PortalCollaboratorSeedData.DefaultPassword);
        var defaultPermissions = PortalModulePermissionCatalog.Serialize(
            PortalModulePermissionCatalog.GetDefaultAssignments(PortalUserRoleCatalog.Collaborator),
            PortalUserRoleCatalog.Collaborator);

        foreach (var entry in PortalCollaboratorSeedData.Entries)
        {
            var existing = await _dbContext.PortalUsers
                .FirstOrDefaultAsync(item => item.Login == entry.Login, cancellationToken);

            if (existing is null)
            {
                _dbContext.PortalUsers.Add(new PortalUser
                {
                    Id = entry.Id,
                    Login = entry.Login,
                    Email = entry.Login,
                    DisplayName = entry.DisplayName,
                    Department = entry.Department,
                    Title = "Colaborador",
                    Role = PortalUserRoleCatalog.Collaborator,
                    ModulePermissionsJson = defaultPermissions,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                continue;
            }

            existing.Email = entry.Login;
            existing.DisplayName = entry.DisplayName;
            existing.Department = entry.Department;
            existing.Title = "Colaborador";
            existing.Role = PortalUserRoleCatalog.Collaborator;
            existing.ModulePermissionsJson = defaultPermissions;
            existing.PasswordHash = passwordHash;
            existing.IsActive = true;
            existing.UpdatedAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
