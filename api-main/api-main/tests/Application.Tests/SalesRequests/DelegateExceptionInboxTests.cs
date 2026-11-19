using System.Security.Claims;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.ApproveException;
using Application.SalesRequests.Commands.Admin.CancelToUnassigned;
using Application.SalesRequests.Commands.Admin.MarkExceptionRead;
using Application.SalesRequests.Commands.Admin.RejectException;
using Application.SalesRequests.Commands.Admin.RequestException;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.Admin.ExceptionInbox;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

public class DelegateExceptionInboxTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Decision_note_is_optional_for_the_delegate()
    {
        await using var db = CreateDb();
        var a = await SeedRequestedAsync(db);
        var b = await SeedRequestedAsync(db);

        var approved = await new ApproveSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = a.Id, Reason = "" }, default);
        var rejected = await new RejectSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new RejectSalesRequestExceptionCommand { RequestId = b.Id, Reason = "   " }, default);

        Assert.True(approved.IsSucceeded);
        Assert.True(rejected.IsSucceeded);
        var savedB = await db.SalesRequests.SingleAsync(x => x.Id == b.Id);
        Assert.Equal(SalesRequestStatus.Rejected, savedB.Status);
        Assert.NotNull(savedB.ExceptionReadAtUtc);
    }

    [Fact]
    public async Task Decision_note_longer_than_500_is_refused()
    {
        await using var db = CreateDb();
        var a = await SeedRequestedAsync(db);

        var result = await new ApproveSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = a.Id, Reason = new string('x', 501) }, default);

        Assert.True(result.IsFailed);
        Assert.Equal(SalesRequestErrors.DecisionNoteTooLong.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Inbox_groups_by_status_and_counts_each_one()
    {
        await using var db = CreateDb();
        var pending = await SeedRequestedAsync(db);
        var approved = await SeedRequestedAsync(db);
        var rejected = await SeedRequestedAsync(db);
        var cancelled = await SeedRequestedAsync(db);
        await SeedAsync(db);

        await new ApproveSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = approved.Id, Reason = "موافق" }, default);
        await new RejectSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new RejectSalesRequestExceptionCommand { RequestId = rejected.Id, Reason = "لا" }, default);
        await new ApproveSalesRequestExceptionHandler(db, Delegate(), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = cancelled.Id, Reason = "" }, default);
        var toCancel = await db.SalesRequests.SingleAsync(x => x.Id == cancelled.Id);
        toCancel.Assign(7, T0.AddHours(3));
        await db.SaveChangesAsync();
        var cancel = await new CancelSalesRequestToUnassignedHandler(db, UserWith(5, SeedPermissions.SalesRequest.CancelToUnassigned), new LaterClock())
            .Handle(new CancelSalesRequestToUnassignedCommand { RequestId = cancelled.Id, Reason = "إلغاء" }, default);
        Assert.True(cancel.IsSucceeded, cancel.Error?.Message);

        var handler = new GetExceptionInboxHandler(db, Delegate());
        var p = await handler.Handle(new GetExceptionInboxQuery { Status = ExceptionInboxStatus.Pending }, default);
        var c = await handler.Handle(new GetExceptionInboxQuery { Status = ExceptionInboxStatus.Cancelled }, default);

        Assert.True(p.IsSucceeded);
        Assert.Equal(new ExceptionInboxCountsDto(1, 1, 1, 1), p.Data!.Counts);
        Assert.Equal(pending.Id, Assert.Single(p.Data.Items).Id);
        Assert.Equal(cancelled.Id, Assert.Single(c.Data!.Items).Id);
        Assert.Equal("Cancelled", c.Data.Items[0].Status);
        Assert.Equal("زبون عليه دين", p.Data.Items[0].RequestReason);
    }

    [Fact]
    public async Task Inbox_respects_branch_scope_and_filter()
    {
        await using var db = CreateDb();
        await SeedRequestedAsync(db);
        var other = await SeedAsync(db, branchId: 2);
        other.HoldForException(SalesRequestExceptionHoldStatus.Pending);
        await db.SaveChangesAsync();

        var handler = new GetExceptionInboxHandler(db, Delegate());
        var all = await handler.Handle(new GetExceptionInboxQuery(), default);
        var outside = await handler.Handle(new GetExceptionInboxQuery { BranchId = 2 }, default);

        Assert.Equal(1, all.Data!.TotalCount);
        Assert.Equal(0, outside.Data!.TotalCount);
    }

    [Fact]
    public async Task Opening_marks_read_once_and_a_new_request_resets_it()
    {
        await using var db = CreateDb();
        var request = await SeedRequestedAsync(db);
        var handler = new MarkExceptionReadHandler(db, Delegate(), new ClockStub());

        var first = await handler.Handle(new MarkExceptionReadCommand(request.Id), default);
        var second = await handler.Handle(new MarkExceptionReadCommand(request.Id), default);

        Assert.True(first.Data!.Changed);
        Assert.False(second.Data!.Changed);
        var row = (await new GetExceptionInboxHandler(db, Delegate()).Handle(new GetExceptionInboxQuery(), default)).Data!.Items.Single();
        Assert.True(row.IsRead);

        var saved = await db.SalesRequests.SingleAsync(x => x.Id == request.Id);
        saved.HoldForException(SalesRequestExceptionHoldStatus.Pending);
        Assert.Null(saved.ExceptionReadAtUtc);
    }

    [Fact]
    public async Task Inbox_and_read_require_exception_permissions()
    {
        await using var db = CreateDb();
        var request = await SeedRequestedAsync(db);
        var stranger = UserWith(9, SeedPermissions.SalesRequest.Assign);

        var list = await new GetExceptionInboxHandler(db, stranger).Handle(new GetExceptionInboxQuery(), default);
        var read = await new MarkExceptionReadHandler(db, stranger, new ClockStub()).Handle(new MarkExceptionReadCommand(request.Id), default);

        Assert.Equal(SalesRequestErrors.ActionForbidden.Code, list.Error!.Value.Code);
        Assert.Equal(SalesRequestErrors.ActionForbidden.Code, read.Error!.Value.Code);
    }

    private static async Task<SalesRequest> SeedRequestedAsync(TestApplicationDbContext db)
    {
        var request = await SeedAsync(db);
        var result = await new RequestSalesRequestExceptionHandler(db, UserWith(5, SeedPermissions.SalesRequest.RequestException), new ClockStub())
            .Handle(new RequestSalesRequestExceptionCommand { RequestId = request.Id, Reason = "زبون عليه دين" }, default);
        Assert.True(result.IsSucceeded, result.Error?.Message);
        return request;
    }

    private static async Task<SalesRequest> SeedAsync(TestApplicationDbContext db, int branchId = 1)
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, branchId, null, null, null);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    private static CurrentUserServiceStub Delegate()
        => new()
        {
            UserId = 6,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("permissions", SeedPermissions.SalesRequest.ExceptionDecide),
                new Claim("permissions", SeedPermissions.SalesRequest.TabExceptions)
            ], "test"))
        };

    private static CurrentUserServiceStub UserWith(int userId, string permission)
        => new()
        {
            UserId = userId,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", permission)], "test"))
        };

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class ClockStub : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => T0.AddHours(2);
    }

    private sealed class LaterClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => T0.AddHours(5);
    }
}
