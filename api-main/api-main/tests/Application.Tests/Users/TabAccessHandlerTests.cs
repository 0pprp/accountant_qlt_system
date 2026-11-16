using Application.Common.AdminPanel;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Commands.Admin.PatchPermissionOverrides;
using Application.Users.Commands.Admin.PatchTabAccess;
using Application.Users.Common;
using Application.Users.Queries.Admin.GetTabAccess;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Tests.Users;

public class TabAccessHandlerTests
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

    private static async Task<(
        TestApplicationDbContext db,
        FakeCurrentUser current,
        CapturingActivityLog logs,
        int targetId,
        int tabPermId)> SeedAsync()
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

        var branch = new Branch("فرع1", province.Id) { Safe = null };
        db.Branches.Add(branch);

        var adminRole = new Role("Admin", "المدير");
        var directorRole = new Role("Director", "المدير المفوض");
        db.Roles.AddRange(adminRole, directorRole);

        var tabPerm = new Permission(
            AdminPanelTabs.PermissionName("saleOverview"),
            "نظرة عامة",
            AdminPanelTabs.Scope,
            AdminPanelTabs.ScopeDisplayName);
        var actionPerm = new Permission("SalesOverview.Read", "قراءة", "SalesOverview", "نظرة");
        var orderPerm = new Permission("Order.Read", "رؤية", "Order", "الطلبات");
        db.Permissions.AddRange(tabPerm, actionPerm, orderPerm);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission(directorRole.Id, tabPerm.Id));

        var admin = new User("a", "a", "a", DateOnly.FromDateTime(DateTime.UtcNow), "admin", "hash", "addr", "phone")
        {
            UserRoles = [new UserRole(adminRole.Id)],
            UserBranches = [new UserBranch(branch.Id)]
        };
        var target = new User("t", "t", "t", DateOnly.FromDateTime(DateTime.UtcNow), "director1", "hash", "addr", "phone")
        {
            UserRoles = [new UserRole(directorRole.Id)],
            UserBranches = [new UserBranch(branch.Id)],
            CreationStep = UserCreationStep.Completed
        };
        db.Users.AddRange(admin, target);
        await db.SaveChangesAsync();

        return (db, new FakeCurrentUser { UserId = admin.Id }, new CapturingActivityLog(), target.Id, tabPerm.Id);
    }

    [Fact]
    public async Task Get_tab_access_shows_role_default_enabled()
    {
        var (db, current, _, targetId, _) = await SeedAsync();
        var handler = new GetUserTabAccessHandler(db, current);
        var result = await handler.Handle(new GetUserTabAccessQuery(targetId), CancellationToken.None);
        Assert.True(result.IsSucceeded);
        var row = result.Data!.Tabs.Single(t => t.TabKey == "saleOverview");
        Assert.True(row.RoleDefaultEnabled);
        Assert.True(row.EffectiveEnabled);
        Assert.Null(row.Override);
    }

    [Fact]
    public async Task Patch_deny_then_reset_null_and_noop_and_409()
    {
        var (db, current, logs, targetId, _) = await SeedAsync();
        var patch = new PatchUserTabAccessHandler(db, current, logs);

        var deny = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = false }]
        }, CancellationToken.None);
        Assert.True(deny.IsSucceeded);

        var target = await db.Users.Include(x => x.UserPermissionDenials).FirstAsync(x => x.Id == targetId);
        Assert.Equal(1, target.PermissionsVersion);
        Assert.Single(target.UserPermissionDenials);

        var stale = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = true }]
        }, CancellationToken.None);
        Assert.True(stale.IsFailed);
        Assert.Equal(UserErrors.PermissionsVersionConflict.Code, stale.Error!.Value.Code);

        var reset = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 1,
            Changes = [new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = null }]
        }, CancellationToken.None);
        Assert.True(reset.IsSucceeded);

        target = await db.Users
            .Include(x => x.UserPermissionDenials)
            .Include(x => x.UserPermissions)
            .FirstAsync(x => x.Id == targetId);
        Assert.Equal(2, target.PermissionsVersion);
        Assert.Empty(target.UserPermissionDenials);

        var noop = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 2,
            Changes = [new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = null }]
        }, CancellationToken.None);
        Assert.True(noop.IsSucceeded);
        target = await db.Users.AsNoTracking().FirstAsync(x => x.Id == targetId);
        Assert.Equal(2, target.PermissionsVersion);
    }

    [Fact]
    public async Task Rejects_not_ready_and_duplicate_keys()
    {
        var (db, current, logs, targetId, _) = await SeedAsync();
        var patch = new PatchUserTabAccessHandler(db, current, logs);

        var bad = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes = [new PatchUserTabAccessChange { TabKey = "committeeRatings", Enabled = true }]
        }, CancellationToken.None);
        Assert.True(bad.IsFailed);
        Assert.Equal(UserErrors.UnknownOrNotReadyTabKey.Code, bad.Error!.Value.Code);

        var dup = await patch.Handle(new PatchUserTabAccessCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes =
            [
                new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = false },
                new PatchUserTabAccessChange { TabKey = "saleOverview", Enabled = true }
            ]
        }, CancellationToken.None);
        Assert.True(dup.IsFailed);
        Assert.Equal(UserErrors.DuplicateTabKeyChange.Code, dup.Error!.Value.Code);
    }

    [Fact]
    public async Task Legacy_patch_rejects_panel_tab_permission()
    {
        var (db, current, logs, targetId, tabPermId) = await SeedAsync();
        var handler = new PatchUserPermissionOverridesHandler(db, current, logs);
        var result = await handler.Handle(new PatchUserPermissionOverridesCommand
        {
            UserId = targetId,
            ExpectedVersion = 0,
            Changes =
            [
                new PermissionOverrideChangeDto
                {
                    PermissionId = tabPermId,
                    Mode = PermissionOverrideMode.Deny
                }
            ]
        }, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Equal(UserErrors.PanelTabManagedViaTabAccess.Code, result.Error!.Value.Code);
    }
}
