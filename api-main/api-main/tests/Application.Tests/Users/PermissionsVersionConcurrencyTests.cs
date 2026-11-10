using Domain.Entities.UserAggregate;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Users;

/// <summary>
/// يثبت تعارض PermissionsVersion على SQLite حقيقي (ليس EF InMemory).
/// </summary>
public class PermissionsVersionConcurrencyTests
{
    private sealed class ConcurrencyDbContext : DbContext
    {
        public ConcurrencyDbContext(DbContextOptions<ConcurrencyDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Ignore(x => x.SalaryDetail);
                entity.Ignore(x => x.OrderListAsMandob);
                entity.Ignore(x => x.OrderListsAsMotaba);
                entity.Ignore(x => x.UserLogins);
                entity.Ignore(x => x.UserRoles);
                entity.Ignore(x => x.Attachments);
                entity.Ignore(x => x.SoldOrders);
                entity.Ignore(x => x.UserPermissions);
                entity.Ignore(x => x.UserPermissionDenials);
                entity.Ignore(x => x.UserBranches);
                entity.Ignore(x => x.SellerCashDeliveryTransactions);
                entity.Property(x => x.PermissionsVersion).IsConcurrencyToken();
            });
        }
    }

    [Fact]
    public async Task Concurrent_updates_same_expected_version_one_succeeds_one_conflicts()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ConcurrencyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new ConcurrencyDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            var user = new User("n", "m", "nc", DateOnly.FromDateTime(DateTime.UtcNow), "u1", "hash", "a", "p");
            user.PermissionsVersion = 1;
            setup.Users.Add(user);
            await setup.SaveChangesAsync();
        }

        await using var ctxA = new ConcurrencyDbContext(options);
        await using var ctxB = new ConcurrencyDbContext(options);

        var userA = await ctxA.Users.FirstAsync();
        var userB = await ctxB.Users.FirstAsync();

        Assert.Equal(1, userA.PermissionsVersion);
        Assert.Equal(1, userB.PermissionsVersion);

        userA.BumpPermissionsVersion();
        await ctxA.SaveChangesAsync();

        userB.BumpPermissionsVersion();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await ctxB.SaveChangesAsync());

        await using var verify = new ConcurrencyDbContext(options);
        var stored = await verify.Users.AsNoTracking().FirstAsync();
        Assert.Equal(2, stored.PermissionsVersion);
    }
}
