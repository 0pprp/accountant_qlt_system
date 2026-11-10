using System.Security.Claims;
using Application.CommitteeTasks.Commands.Admin.Upsert;
using Application.CommitteeTasks.Queries;
using Application.Common.Models.TokenFactory;
using Domain.Entities.CommitteeTaskAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.CommitteeTasks;

public class CommitteeTaskHandlerTests
{
    private static ClaimsPrincipal Principal(params string[] permissions)
        => new(new ClaimsIdentity(
            [new Claim(CustomClaims.Permissions, string.Join(",", permissions))],
            "test"));

    private static async Task<(TestApplicationDbContext db, CurrentUserServiceStub current, DateTimeProviderStub clock)>
        SetupAsync(params string[] permissions)
    {
        var db = new TestApplicationDbContext(
            new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        await db.Database.EnsureCreatedAsync();
        var user = new User("مدير اختبار", "م", "n1", DateOnly.FromDateTime(DateTime.UtcNow), "mgr", "hash", "a", "p");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var current = new CurrentUserServiceStub
        {
            UserId = user.Id,
            User = Principal(permissions)
        };
        return (db, current, new DateTimeProviderStub());
    }

    [Fact]
    public async Task Create_assigns_sort_and_persists()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.CommitteeTask.Write);
        var handler = new CreateCommitteeTaskHandler(db, current, clock);
        var result = await handler.Handle(new CreateCommitteeTaskCommand
        {
            Body = "مهمة أولى",
            IsEnabled = true
        }, CancellationToken.None);
        Assert.True(result.IsSucceeded);
        Assert.Equal(0, result.Data!.SortOrder);
        Assert.Equal(1, await db.CommitteeTaskItems.CountAsync());
    }

    [Fact]
    public async Task Update_rejects_stale_version()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.CommitteeTask.Write);
        db.CommitteeTaskItems.Add(new CommitteeTaskItem("نص", true, 0, 1, "م", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var id = await db.CommitteeTaskItems.Select(x => x.Id).FirstAsync();

        var handler = new UpdateCommitteeTaskHandler(db, current, clock);
        var result = await handler.Handle(new UpdateCommitteeTaskCommand
        {
            Id = id,
            Body = "تعديل",
            IsEnabled = true,
            ExpectedVersion = 99
        }, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Equal("CommitteeTask.Concurrency", result.Error!.Value.Code);
    }

    [Fact]
    public async Task App_list_returns_enabled_only()
    {
        var (db, current, _) = await SetupAsync(SeedPermissions.CommitteeTask.AppRead);
        db.CommitteeTaskItems.Add(new CommitteeTaskItem("مفعّلة", true, 1, 1, "م", DateTimeOffset.UtcNow));
        db.CommitteeTaskItems.Add(new CommitteeTaskItem("موقوفة", false, 0, 1, "م", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var handler = new ListAppCommitteeTasksHandler(db, current);
        var result = await handler.Handle(new ListAppCommitteeTasksQuery(), CancellationToken.None);
        Assert.True(result.IsSucceeded);
        Assert.Single(result.Data!);
        Assert.Equal("مفعّلة", result.Data![0].Body);
    }

    [Fact]
    public async Task App_list_forbidden_without_app_read()
    {
        var (db, current, _) = await SetupAsync();
        var handler = new ListAppCommitteeTasksHandler(db, current);
        var result = await handler.Handle(new ListAppCommitteeTasksQuery(), CancellationToken.None);
        Assert.True(result.IsFailed);
    }
}
