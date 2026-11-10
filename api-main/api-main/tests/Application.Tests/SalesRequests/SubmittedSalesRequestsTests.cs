using System.Security.Claims;
using Application.SalesRequests.Queries.App.Submitted;
using SeedPermissions = Application.Common.SeedData.Permissions;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.SalesRequests;

public class SubmittedSalesRequestsTests
{
    private static TestApplicationDbContext Db() => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static CurrentUserServiceStub Actor(string permission = SeedPermissions.SalesRequest.SubmitDelegate) => new() {
        UserId = 1, BranchIds = [1], User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", permission)], "test"))
    };
    private static async Task Seed(TestApplicationDbContext db) {
        db.Branches.AddRange(new Branch("one", 1) { Id = 1 }, new Branch("two", 1) { Id = 2 });
        var u = new User("name", "mother", "code", new DateOnly(1990,1,1), "u", "hash", "address", "07800000000") { Id = 1 };
        u.UserBranches.Add(new UserBranch(1)); db.Users.Add(u); await db.SaveChangesAsync();
    }
    private static SalesRequest Row(int owner = 1, int branch = 1, SalesRequestSourceType source = SalesRequestSourceType.Delegate,
        SalesRequestStatus status = SalesRequestStatus.New, bool deleted = false) => new("name", "07800000000", source, branch, null, null, owner) {
            Status = status, IsDeleted = deleted, DeleteReason = deleted ? "removed by manager" : null, CreatedAt = DateTimeOffset.UtcNow
        };
    [Fact] public async Task Scope_requires_creator_source_and_current_branch_membership() {
        using var db = Db(); await Seed(db);
        db.SalesRequests.AddRange(Row(), Row(owner:2), Row(branch:2), Row(source:SalesRequestSourceType.Follower)); await db.SaveChangesAsync();
        var result = await new GetSubmittedSalesRequestsHandler(db,Actor()).Handle(new(SalesRequestSourceType.Delegate), default);
        Assert.True(result.IsSucceeded); Assert.Single(result.Data!.Items);
        var actor = Actor(); actor.BranchIds = [2];
        Assert.Empty((await new GetSubmittedSalesRequestsHandler(db,actor).Handle(new(SalesRequestSourceType.Delegate),default)).Data!.Items);
    }
    [Fact] public async Task Revoked_database_branch_is_not_granted_by_stale_claim() {
        using var db = Db(); await Seed(db); db.SalesRequests.Add(Row()); await db.SaveChangesAsync();
        var user = await db.Users.SingleAsync(); user.UserBranches.Clear(); await db.SaveChangesAsync();
        var result = await new GetSubmittedSalesRequestsHandler(db,Actor()).Handle(new(SalesRequestSourceType.Delegate), default);
        Assert.Empty(result.Data!.Items);
    }
    [Fact] public async Task Empty_branch_scope_and_other_source_permission_are_denied() {
        using var db = Db(); await Seed(db);
        var actor = Actor(); actor.BranchIds=[];
        Assert.True((await new GetSubmittedSalesRequestsHandler(db,actor).Handle(new(SalesRequestSourceType.Delegate),default)).IsFailed);
        Assert.True((await new GetSubmittedSalesRequestsHandler(db,Actor()).Handle(new(SalesRequestSourceType.Follower),default)).IsFailed);
    }
    [Theory] [InlineData("invalid",1)] [InlineData("all",0)] [InlineData("all",10001)]
    public async Task Invalid_filter_or_page_is_rejected(string tab,int page) {
        using var db=Db();
        Assert.True((await new GetSubmittedSalesRequestsHandler(db,Actor()).Handle(new(SalesRequestSourceType.Delegate,tab,page),default)).IsFailed);
    }
    [Fact] public async Task Removed_incomplete_request_is_rejected_with_reason_but_completed_removal_is_hidden() {
        using var db=Db(); await Seed(db);
        db.SalesRequests.AddRange(Row(deleted:true), Row(status:SalesRequestStatus.Completed,deleted:true), Row(status:SalesRequestStatus.Completed)); await db.SaveChangesAsync();
        var handler=new GetSubmittedSalesRequestsHandler(db,Actor());
        var rejected=(await handler.Handle(new(SalesRequestSourceType.Delegate,"rejected"),default)).Data!;
        Assert.Equal("removed by manager",Assert.Single(rejected.Items).RejectionReason);
        Assert.Single((await handler.Handle(new(SalesRequestSourceType.Delegate,"completed"),default)).Data!.Items);
        Assert.Empty((await handler.Handle(new(SalesRequestSourceType.Delegate,"inProgress"),default)).Data!.Items);
    }
    [Fact] public async Task Paging_is_bounded_without_dropping_remaining_results() {
        using var db=Db(); await Seed(db);
        for(var i=0;i<35;i++)db.SalesRequests.Add(Row()); await db.SaveChangesAsync();
        var h=new GetSubmittedSalesRequestsHandler(db,Actor());
        var a=(await h.Handle(new(SalesRequestSourceType.Delegate),default)).Data!;
        var b=(await h.Handle(new(SalesRequestSourceType.Delegate,"all",2),default)).Data!;
        Assert.Equal(30,a.Items.Count); Assert.True(a.HasMore); Assert.Equal(5,b.Items.Count); Assert.False(b.HasMore);
        Assert.Empty(a.Items.Select(x=>x.RequestId).Intersect(b.Items.Select(x=>x.RequestId)));
    }
}

