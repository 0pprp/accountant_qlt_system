using System.Security.Claims;
using Application.BranchVisits.Queries;
using Application.Common.Models.TokenFactory;
using Domain.Entities.BranchAggregate;
using Domain.Entities.BranchVisitAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.BranchVisits;

public class ListBranchVisitsHandlerTests
{
    [Fact]
    public async Task List_returns_branch_name_and_filters_by_branch()
    {
        await using var db = CreateDb();
        var a = new Branch("فرع أ", 1);
        var b = new Branch("فرع ب", 1);
        db.Branches.AddRange(a, b);
        await db.SaveChangesAsync();
        db.BranchVisits.AddRange(
            new BranchVisit(a.Id, 1, "مفوض", DateTimeOffset.UtcNow.AddHours(-2)),
            new BranchVisit(b.Id, 1, "مفوض", DateTimeOffset.UtcNow.AddHours(-1)));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.BranchVisit.Read, [a.Id, b.Id]);
        var all = await new ListBranchVisitsHandler(db, current).Handle(new ListBranchVisitsQuery { PageSize = 100 }, default);
        Assert.Equal(2, all.Data!.TotalCount);
        Assert.Equal("فرع ب", all.Data.Items[0].BranchName);

        var onlyA = await new ListBranchVisitsHandler(db, current)
            .Handle(new ListBranchVisitsQuery { BranchId = a.Id, PageSize = 100 }, default);
        var visit = Assert.Single(onlyA.Data!.Items);
        Assert.Equal(a.Id, visit.BranchId);
        Assert.Equal("فرع أ", visit.BranchName);
    }

    [Fact]
    public async Task List_branch_filter_outside_scope_returns_empty()
    {
        await using var db = CreateDb();
        var a = new Branch("فرع أ", 1);
        var b = new Branch("فرع ب", 1);
        db.Branches.AddRange(a, b);
        await db.SaveChangesAsync();
        db.BranchVisits.Add(new BranchVisit(b.Id, 1, "مفوض", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.BranchVisit.Read, [a.Id]);
        var result = await new ListBranchVisitsHandler(db, current)
            .Handle(new ListBranchVisitsQuery { BranchId = b.Id }, default);

        Assert.True(result.IsSucceeded);
        Assert.Empty(result.Data!.Items);
        Assert.Equal(0, result.Data.TotalCount);
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CurrentUserServiceStub User(string permissions, List<int> branchIds, int userId = 1)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(CustomClaims.Permissions, permissions)
        ], "test");
        return new CurrentUserServiceStub
        {
            UserId = userId,
            User = new ClaimsPrincipal(identity),
            BranchIds = branchIds
        };
    }
}
