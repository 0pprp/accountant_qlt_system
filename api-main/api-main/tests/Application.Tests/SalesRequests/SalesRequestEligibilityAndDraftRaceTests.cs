using System.Security.Claims;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.Assign;
using Application.SalesRequests.Commands.App.EnsureDraft;
using Application.SalesRequests.Commands.App.TransferName;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.App.TransferPeers;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SalesEmployeeAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

public class SalesRequestEligibilityAndDraftRaceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Eligibility_requires_active_assignment_and_app_read_respecting_deny()
    {
        await using var db = Db();
        var appRead = await EnsureAppReadAsync(db);

        var noAssignment = await AddUserAsync(db, "no-asg");
        db.UserPermissions.Add(new UserPermission(noAssignment.Id, appRead.Id));
        await db.SaveChangesAsync();
        Assert.False(await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            db, noAssignment.Id, 1, T0, default));

        var ended = await AddUserAsync(db, "ended");
        db.UserPermissions.Add(new UserPermission(ended.Id, appRead.Id));
        var endedAsg = new SalesEmployeeAssignment(ended.Id, 1, T0.AddDays(-10), 1);
        endedAsg.End(T0.AddDays(-1));
        db.SalesEmployeeAssignments.Add(endedAsg);
        await db.SaveChangesAsync();
        Assert.False(await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            db, ended.Id, 1, T0, default));

        var denied = await AddUserAsync(db, "denied");
        db.UserPermissions.Add(new UserPermission(denied.Id, appRead.Id));
        db.UserPermissionDenials.Add(new UserPermissionDenial(denied.Id, appRead.Id) { Permission = appRead });
        db.SalesEmployeeAssignments.Add(new SalesEmployeeAssignment(denied.Id, 1, T0.AddDays(-1), 1));
        await db.SaveChangesAsync();
        Assert.False(await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            db, denied.Id, 1, T0, default));

        var ok = await AddEligibleAsync(db, "ok");
        Assert.True(await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            db, ok.Id, 1, T0, default));
    }

    private static Customer MakeCustomer() => new(
        "زبون",
        "أم الزبون",
        "9876543210",
        new DateOnly(1985, 5, 5),
        "07801112233",
        "07801112233",
        new Business("محل", "العنوان", "علامة"),
        1);

    [Fact]
    public async Task Transfer_rejects_peer_without_assignment_or_denied_app_read()
    {
        await using var db = Db();
        var owner = await AddEligibleAsync(db, "owner");
        var peerNoAsg = await AddUserAsync(db, "peer");
        var appRead = await EnsureAppReadAsync(db);
        db.UserPermissions.Add(new UserPermission(peerNoAsg.Id, appRead.Id));

        var request = new SalesRequest("زبون", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        request.Assign(owner.Id, T0);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var actor = Actor(owner.Id, SeedPermissions.SalesRequest.AppTransferName);
        var transfer = await new TransferSalesRequestNameHandler(db, actor, new ClockStub(T0))
            .Handle(new TransferSalesRequestNameCommand
            {
                RequestId = request.Id,
                ToEmployeeId = peerNoAsg.Id,
                Reason = "نقل لغير مكلّف"
            }, default);
        Assert.Equal(SalesRequestErrors.TransferPeerInvalid.Code, transfer.Error!.Value.Code);

        var peers = await new GetTransferPeersHandler(db, actor, new ClockStub(T0))
            .Handle(new GetTransferPeersQuery(request.Id), default);
        Assert.True(peers.IsSucceeded);
        Assert.DoesNotContain(peers.Data!, p => p.Id == peerNoAsg.Id);
    }

    [Fact]
    public async Task Ensure_draft_rejects_after_reassignment_without_orphan_order()
    {
        await using var db = Db();
        var a = await AddEligibleAsync(db, "a");
        var b = await AddEligibleAsync(db, "b");
        var customer = MakeCustomer();
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var request = new SalesRequest("زبون", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        request.Assign(a.Id, T0);
        request.ExistingCustomerId = customer.Id;
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        // إعادة إسناد قبل المسودة
        request.Reassign(b.Id, "نقل", T0.AddMinutes(1));
        await db.SaveChangesAsync();

        var ordersBefore = await db.Orders.CountAsync();
        var draft = await new EnsureSalesRequestDraftHandler(db, Actor(a.Id, SeedPermissions.SalesRequest.AppLinkDraft), new ClockStub(T0.AddMinutes(2)))
            .Handle(new EnsureSalesRequestDraftCommand(request.Id), default);
        Assert.Equal(SalesRequestErrors.NotOwner.Code, draft.Error!.Value.Code);
        Assert.Equal(ordersBefore, await db.Orders.CountAsync());
        Assert.Empty(db.SalesRequestChangeOutboxes);
    }

    [Fact]
    public async Task Ensure_draft_idempotent_same_linked_order()
    {
        await using var db = Db();
        var a = await AddEligibleAsync(db, "a");
        var customer = MakeCustomer();
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var request = new SalesRequest("زبون", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        request.Assign(a.Id, T0);
        request.ExistingCustomerId = customer.Id;
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var handler = new EnsureSalesRequestDraftHandler(
            db, Actor(a.Id, SeedPermissions.SalesRequest.AppLinkDraft), new ClockStub(T0.AddMinutes(1)));
        var first = await handler.Handle(new EnsureSalesRequestDraftCommand(request.Id), default);
        Assert.True(first.IsSucceeded);
        Assert.True(first.Data!.LinkedOrderId is > 0);

        var second = await handler.Handle(new EnsureSalesRequestDraftCommand(request.Id), default);
        Assert.True(second.IsSucceeded);
        Assert.Equal(first.Data.LinkedOrderId, second.Data!.LinkedOrderId);
        Assert.Equal(1, await db.Orders.CountAsync());
    }

    [Fact]
    public async Task Assign_and_transfer_share_same_eligible_query()
    {
        await using var db = Db();
        var eligible = await AddEligibleAsync(db, "el");
        var outsider = await AddUserAsync(db, "out");
        var appRead = await EnsureAppReadAsync(db);
        db.UserPermissions.Add(new UserPermission(outsider.Id, appRead.Id));
        await db.SaveChangesAsync();

        var ids = await SalesRequestEmployeeEligibility.QueryEligibleAssignees(db, 1, T0)
            .Select(x => x.Id).ToListAsync();
        Assert.Contains(eligible.Id, ids);
        Assert.DoesNotContain(outsider.Id, ids);

        var request = new SalesRequest("زبون", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var bad = await new AssignSalesRequestHandler(db, Actor(9, SeedPermissions.SalesRequest.Assign), new ClockStub(T0))
            .Handle(new AssignSalesRequestCommand { RequestId = request.Id, EmployeeId = outsider.Id }, default);
        Assert.Equal(SalesRequestErrors.InvalidEmployee.Code, bad.Error!.Value.Code);

        var ok = await new AssignSalesRequestHandler(db, Actor(9, SeedPermissions.SalesRequest.Assign), new ClockStub(T0))
            .Handle(new AssignSalesRequestCommand { RequestId = request.Id, EmployeeId = eligible.Id }, default);
        Assert.True(ok.IsSucceeded);
    }

    private static TestApplicationDbContext Db() => new(
        new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static CurrentUserServiceStub Actor(int userId, params string[] permissions) => new()
    {
        UserId = userId,
        BranchIds = [1],
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permissions", string.Join(",", permissions))], "test"))
    };

    private static async Task<Permission> EnsureAppReadAsync(TestApplicationDbContext db)
    {
        var p = await db.Permissions.FirstOrDefaultAsync(x => x.Name == SeedPermissions.SalesRequest.AppRead);
        if (p is not null) return p;
        p = new Permission(SeedPermissions.SalesRequest.AppRead, "قراءة", "SalesRequest", "طلبات");
        db.Permissions.Add(p);
        await db.SaveChangesAsync();
        return p;
    }

    private static async Task<User> AddUserAsync(TestApplicationDbContext db, string tag)
    {
        var u = new User("موظف " + tag, "أم", "n", new DateOnly(1990, 1, 1), $"u-{tag}-{Guid.NewGuid():N}", "hash", "عنوان", "0780")
        {
            CreationStep = UserCreationStep.Completed
        };
        u.UserBranches.Add(new UserBranch(1));
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    private static async Task<User> AddEligibleAsync(TestApplicationDbContext db, string tag)
    {
        var u = await AddUserAsync(db, tag);
        var appRead = await EnsureAppReadAsync(db);
        db.UserPermissions.Add(new UserPermission(u.Id, appRead.Id) { Permission = appRead });
        db.SalesEmployeeAssignments.Add(new SalesEmployeeAssignment(u.Id, 1, T0.AddDays(-1), 1));
        await db.SaveChangesAsync();
        return u;
    }

    private sealed class ClockStub(DateTimeOffset now) : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => now;
    }
}
