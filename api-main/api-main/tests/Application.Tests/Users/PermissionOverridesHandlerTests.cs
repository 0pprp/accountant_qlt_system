using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Commands.Admin.PatchPermissionOverrides;
using Application.Users.Commands.Admin.UpdatePermissions;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Tests.Users;

public class PermissionOverridesHandlerTests
{
    private sealed class FakeCurrentUser : ICurrentUserService
    {
        public int? UserId { get; set; }
        public ClaimsPrincipal? User { get; set; }
        public string? Role { get; set; }
        public List<int>? BranchIds { get; set; }
    }

    private sealed class CapturingActivityLog : IActivityLogService
    {
        public List<string?> Details { get; } = [];

        public Task AddAsync(
            int branchId,
            ActivityType activityType,
            TargetEntityType targetEntityType,
            int? targetEntityId,
            ActivityLogActor? actorOverride = null,
            string? details = null)
        {
            Details.Add(details);
            return Task.CompletedTask;
        }
    }

    private static async Task<(TestApplicationDbContext db, FakeCurrentUser current, CapturingActivityLog logs, int adminId, int targetId, int permId)> SeedAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new TestApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

        var province = new Domain.Entities.ProvinceAggregate.Province("بغداد");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var branch = new Branch("فرع1", province.Id);
        branch.Safe = null;
        db.Branches.Add(branch);

        var adminRole = new Role("Admin", "المدير");
        var mandobRole = new Role("Mandob", "المندوب");
        db.Roles.AddRange(adminRole, mandobRole);

        var perm = new Permission("Order.Read", "رؤية", "Order", "الطلبات");
        db.Permissions.Add(perm);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission(mandobRole.Id, perm.Id));

        var admin = new User("a", "a", "a", DateOnly.FromDateTime(DateTime.UtcNow), "admin", "hash", "addr", "phone")
        {
            UserRoles = [new UserRole(adminRole.Id)],
            UserBranches = [new UserBranch(branch.Id)]
        };
        var otherAdmin = new User("b", "b", "b", DateOnly.FromDateTime(DateTime.UtcNow), "admin2", "hash", "addr", "phone")
        {
            UserRoles = [new UserRole(adminRole.Id)],
            UserBranches = [new UserBranch(branch.Id)]
        };
        var target = new User("t", "t", "t", DateOnly.FromDateTime(DateTime.UtcNow), "target", "hash", "addr", "phone")
        {
            UserRoles = [new UserRole(mandobRole.Id)],
            UserBranches = [new UserBranch(branch.Id)],
            CreationStep = UserCreationStep.Permissions
        };
        db.Users.AddRange(admin, otherAdmin, target);
        await db.SaveChangesAsync();

        return (db, new FakeCurrentUser { UserId = admin.Id }, new CapturingActivityLog(), otherAdmin.Id, target.Id, perm.Id);
    }

    [Fact]
    public async Task Non_super_admin_is_rejected()
    {
        var (db, current, logs, _, targetId, permId) = await SeedAsync();
        current.UserId = targetId;
        var handler = new PatchUserPermissionOverridesHandler(db, current, logs);
        var result = await handler.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PermissionOverrideChangeDto { PermissionId = permId, Mode = PermissionOverrideMode.Deny }]
        }, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Equal(UserErrors.NotAuthorizedToManagePermissions.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Protected_admin_cannot_be_patched()
    {
        var (db, current, logs, protectedAdminId, _, permId) = await SeedAsync();
        var handler = new PatchUserPermissionOverridesHandler(db, current, logs);
        var result = await handler.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = protectedAdminId,
            ExpectedVersion = 0,
            Changes = [new PermissionOverrideChangeDto { PermissionId = permId, Mode = PermissionOverrideMode.Deny }]
        }, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Equal(UserErrors.ProtectedAccount.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Patch_then_stale_put_fails_without_losing_denial()
    {
        var (db, current, logs, _, targetId, permId) = await SeedAsync();
        var patch = new PatchUserPermissionOverridesHandler(db, current, logs);
        var patchResult = await patch.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PermissionOverrideChangeDto { PermissionId = permId, Mode = PermissionOverrideMode.Deny }]
        }, CancellationToken.None);
        Assert.True(patchResult.IsSucceeded);

        var target = await db.Users.Include(x => x.UserPermissionDenials).FirstAsync(x => x.Id == targetId);
        Assert.Equal(1, target.PermissionsVersion);
        Assert.Single(target.UserPermissionDenials);

        var put = new UpdateUserPermissionsHandler(db, current, logs);
        var putResult = await put.Handle(new UpdateUserPermissionsCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            PermissionIds = []
        }, CancellationToken.None);
        Assert.True(putResult.IsFailed);
        Assert.Equal(UserErrors.PermissionsVersionConflict.Code, putResult.Error!.Value.Code);

        target = await db.Users.AsNoTracking().Include(x => x.UserPermissionDenials).FirstAsync(x => x.Id == targetId);
        Assert.Single(target.UserPermissionDenials);
        Assert.Equal(1, target.PermissionsVersion);
    }

    [Fact]
    public async Task No_op_patch_does_not_bump_version_or_write_audit_details()
    {
        var (db, current, logs, _, targetId, permId) = await SeedAsync();
        var handler = new PatchUserPermissionOverridesHandler(db, current, logs);
        var result = await handler.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PermissionOverrideChangeDto { PermissionId = permId, Mode = PermissionOverrideMode.Inherit }]
        }, CancellationToken.None);
        Assert.True(result.IsSucceeded);
        var target = await db.Users.AsNoTracking().FirstAsync(x => x.Id == targetId);
        Assert.Equal(0, target.PermissionsVersion);
        Assert.Empty(logs.Details);
    }

    [Fact]
    public async Task Deny_then_effective_login_shape_excludes_permission()
    {
        var (db, current, logs, _, targetId, permId) = await SeedAsync();
        var handler = new PatchUserPermissionOverridesHandler(db, current, logs);
        await handler.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PermissionOverrideChangeDto { PermissionId = permId, Mode = PermissionOverrideMode.Deny }]
        }, CancellationToken.None);

        var user = await db.Users.AsNoTracking()
            .Where(x => x.Id == targetId)
            .Select(x => new
            {
                Role = x.UserRoles.SelectMany(ur => ur.Role!.RolePermissions.Select(rp => rp.Permission!)).ToList(),
                Allow = x.UserPermissions.Select(up => up.Permission!).ToList(),
                Deny = x.UserPermissionDenials.Select(d => d.Permission!).ToList()
            })
            .FirstAsync();

        var effective = EffectivePermissionCalculator.Compute(user.Role, user.Allow, user.Deny);
        Assert.DoesNotContain(effective, p => p.Id == permId);
    }
}
