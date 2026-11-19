using Application.Common.AdminPanel;
using Application.Users.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Users;

public class PanelTabMigrationTests
{
    [Fact]
    public async Task Dry_run_marks_full_bundle_open_and_partial_review()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new TestApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

        var province = new Domain.Entities.ProvinceAggregate.Province("بغداد");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();
        var branch = new Branch("فرع", province.Id) { Safes = [] };
        db.Branches.Add(branch);

        var role = new Role("Mandob", "مندوب");
        db.Roles.Add(role);

        foreach (var p in AdminPanelTabs.OutsideAll)
            db.Permissions.Add(p);
        db.Permissions.Add(new Permission("SalesOverview.Read", "قراءة", "SalesOverview", "نظرة"));
        db.Permissions.Add(new Permission("Order.Read", "رؤية", "Order", "مبيعات"));
        await db.SaveChangesAsync();

        var overviewRead = await db.Permissions.SingleAsync(p => p.Name == "SalesOverview.Read");
        var orderRead = await db.Permissions.SingleAsync(p => p.Name == "Order.Read");
        db.RolePermissions.Add(new RolePermission(role.Id, overviewRead.Id));
        // partial for saleOrders: only Order.Read but saleOrders bundle is just Order.Read → open
        db.RolePermissions.Add(new RolePermission(role.Id, orderRead.Id));

        var user = new User("u", "u", "u", DateOnly.FromDateTime(DateTime.UtcNow), "mig1", "hash", "a", "p")
        {
            UserRoles = [new UserRole(role.Id)],
            UserBranches = [new UserBranch(branch.Id)]
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await PanelTabAccountMigrator.RunAsync(db, apply: false, snapshotDirectory: null);
        Assert.False(result.Applied);
        Assert.Equal(1, result.UsersScanned);
        Assert.Contains(result.ReviewRows, r => r.TabKey == "saleOverview" && r.Decision == "open");
        Assert.Contains(result.ReviewRows, r => r.TabKey == "saleOrders" && r.Decision == "open");
        Assert.Contains(result.ReviewRows, r => r.TabKey == "sr.incoming" && r.Decision == "closed");
    }
}
