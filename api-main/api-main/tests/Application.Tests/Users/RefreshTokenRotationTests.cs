using Application.Users.Common;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Users;

public class RefreshTokenRotationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 18, 0, 0, TimeSpan.Zero);
    private const int UserId = 7;

    [Fact]
    public async Task Old_token_stays_valid_during_grace_so_a_lost_response_does_not_lock_the_phone()
    {
        await using var db = Db();
        var sessionEnd = Now.AddDays(20);
        db.UserLogins.Add(new UserLogin("old", sessionEnd, UserId));
        await db.SaveChangesAsync();

        await RefreshTokenRotation.RotateAsync(db, UserId, "old", "new", Now, default);

        var old = await db.UserLogins.SingleAsync(x => x.RefreshToken == "old");
        var fresh = await db.UserLogins.SingleAsync(x => x.RefreshToken == "new");
        Assert.Equal(Now + RefreshTokenRotation.Grace, old.RefreshTokenExpiresAt);
        Assert.Equal(sessionEnd, fresh.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task Reusing_the_old_token_in_grace_keeps_the_full_session_length()
    {
        await using var db = Db();
        var sessionEnd = Now.AddDays(20);
        db.UserLogins.Add(new UserLogin("old", sessionEnd, UserId));
        await db.SaveChangesAsync();

        await RefreshTokenRotation.RotateAsync(db, UserId, "old", "lost", Now, default);
        await RefreshTokenRotation.RotateAsync(db, UserId, "old", "retry", Now.AddMinutes(5), default);

        Assert.Equal(sessionEnd, (await db.UserLogins.SingleAsync(x => x.RefreshToken == "retry")).RefreshTokenExpiresAt);
        Assert.Equal(sessionEnd, (await db.UserLogins.SingleAsync(x => x.RefreshToken == "lost")).RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task Expired_rows_of_the_user_are_removed_and_other_users_are_untouched()
    {
        await using var db = Db();
        db.UserLogins.AddRange(
            new UserLogin("old", Now.AddDays(10), UserId),
            new UserLogin("stale", Now.AddHours(-1), UserId),
            new UserLogin("someone-else", Now.AddHours(-1), 99));
        await db.SaveChangesAsync();

        await RefreshTokenRotation.RotateAsync(db, UserId, "old", "new", Now, default);

        Assert.False(await db.UserLogins.AnyAsync(x => x.RefreshToken == "stale"));
        Assert.True(await db.UserLogins.AnyAsync(x => x.RefreshToken == "someone-else"));
    }

    [Fact]
    public async Task Unknown_token_changes_nothing()
    {
        await using var db = Db();
        db.UserLogins.Add(new UserLogin("old", Now.AddDays(10), UserId));
        await db.SaveChangesAsync();

        await RefreshTokenRotation.RotateAsync(db, UserId, "missing", "new", Now, default);

        Assert.Single(await db.UserLogins.ToListAsync());
    }

    private static TestApplicationDbContext Db() => new(
        new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
