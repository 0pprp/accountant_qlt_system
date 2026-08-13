using Application.Common.Interfaces;
using Application.Common.SeedData;
using Application.Common.Settings;
using Application.Common.Utilities;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class DatabaseInitializer
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly AdminData _adminData;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DatabaseInitializer(IApplicationDbContext dbContext, ILogger<DatabaseInitializer> logger, AdminData adminData,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _adminData = adminData;
        _dateTimeProvider = dateTimeProvider;
    }

    public virtual async Task CreateDatabaseAsync()
    {
        try
        {
            await _dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to create database and apply migrations. details: {exceptionMessage}", ex.Message);
            throw;
        }
    }

    public virtual async Task SeedDataAsync()
    {
        try
        {
            await SeedPermissionsAsync();

            await SeedRolesAsync();

            await SeedRolePermissionsAsync();

            await SeedSuperAdminAsync();

            await SeedMainSafeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to seed admin data. details: {exceptionMessage}", ex.Message);
            throw;
        }
    }

    private async Task SeedPermissionsAsync()
    {
        foreach (var permission in Permissions.All)
        {
            var existingPermission = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Name == permission.Name);
            if (existingPermission is not null)
            {
                existingPermission.Update(permission.DisplayName, permission.Scope, permission.ScopeDisplayName);
                continue;
            }

            _dbContext.Permissions.Add(permission);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = Roles.All.Select(x => new Role(x.Name, x.DisplayName)).ToList();
        foreach (var role in roles)
        {
            var isRoleExist = await _dbContext.Roles.AnyAsync(x => x.Name == role.Name);
            if (isRoleExist)
                continue;

            _dbContext.Roles.Add(role);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedRolePermissionsAsync()
    {
        var allPermissions = await _dbContext.Permissions.ToListAsync();
        var roles = await _dbContext.Roles.ToListAsync();
        foreach (var rolePermissionDto in RolePermissions.All)
        {
            var roleId = roles.First(x => x.Name == rolePermissionDto.RoleName).Id;
            var permissions = allPermissions.Where(x => rolePermissionDto.Permissions.Contains(x.Name)).ToList();

            foreach (var permission in permissions)
            {
                var isRolePermissionExist = await _dbContext.RolePermissions
                    .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permission.Id);
                if (isRolePermissionExist)
                    continue;

                var rolePermission = new RolePermission(roleId, permission.Id);
                _dbContext.RolePermissions.Add(rolePermission);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedSuperAdminAsync()
    {
        var superAdminRole = await _dbContext.Roles.FirstAsync(x => x.Name == Roles.Admin.Name);

        var hashedPassword = PasswordHash.HashPassword(_adminData.Password);
        var superAdmin = new User(_adminData.Username, _adminData.Username, _adminData.Username, _dateTimeProvider.Today,
            _adminData.Username, hashedPassword, string.Empty, _adminData.Username)
        {
            UserRoles = [new UserRole(superAdminRole.Id)]
        };
        var isSuperAdminExist = await _dbContext.Users.AnyAsync(x => x.Username == _adminData.Username);
        if (isSuperAdminExist)
            return;

        _dbContext.Users.Add(superAdmin);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedMainSafeAsync()
    {
        const string safeName = "القاصة الرئيسية";
        var isSafeExist = await _dbContext.Safes.AnyAsync(x => x.Name == safeName);
        if (isSafeExist)
            return;
        
        var safe = new Safe(safeName);
        _dbContext.Safes.Add(safe);
        await _dbContext.SaveChangesAsync();
    }
}