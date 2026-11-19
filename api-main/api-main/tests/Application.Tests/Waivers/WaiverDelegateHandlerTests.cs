using System.Security.Claims;
using Application.Common.Models.TokenFactory;
using Application.Waivers.Commands.Decide;
using Application.Waivers.Common;
using Application.Waivers.Queries;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.WaiverAggregate;
using Domain.Entities.WaiverAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Waivers;

public class WaiverDelegateHandlerTests
{
    private const int ScopedBranch = 1;
    private const int OtherBranch = 2;
    private const int DelegateId = 50;
    private const int RequesterId = 10;

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task Approve_rejects_non_positive_or_invalid_amount(double amount)
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, ScopedBranch);

        var result = await Approve(db, Delegate(), waiver.Id, amount, 0);

        Assert.True(result.IsFailed);
        Assert.Equal(WaiverErrors.AmountRequired.Code, result.Error!.Value.Code);
        var stored = await db.CustomerWaiverRequests.AsNoTracking().SingleAsync();
        Assert.Equal(WaiverRequestStatus.Pending, stored.Status);
        Assert.Null(stored.ApprovedAmount);
    }

    [Fact]
    public async Task Approve_moves_pending_to_awaiting_accountant_and_records_decision()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, ScopedBranch);

        var result = await Approve(db, Delegate(), waiver.Id, 1_500_000, 0, "  موافقة  ");

        Assert.True(result.IsSucceeded);
        Assert.Equal("AwaitingAccountant", result.Data!.Status);
        var stored = await db.CustomerWaiverRequests.AsNoTracking().SingleAsync();
        Assert.Equal(WaiverRequestStatus.AwaitingAccountant, stored.Status);
        Assert.Equal(1_500_000, stored.ApprovedAmount);
        Assert.Equal("موافقة", stored.DecisionNote);
        Assert.False(stored.Applied);
        Assert.Equal(1, stored.ConcurrencyStamp);
        Assert.Single(await db.CustomerDecisions.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Approve_only_from_pending()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, ScopedBranch);
        var first = await Reject(db, Delegate(), waiver.Id, 0, "مرفوض");
        Assert.True(first.IsSucceeded);

        var again = await Approve(db, Delegate(), waiver.Id, 100_000, 1);

        Assert.True(again.IsFailed);
        Assert.Equal(WaiverErrors.InvalidTransition.Code, again.Error!.Value.Code);
        var stored = await db.CustomerWaiverRequests.AsNoTracking().SingleAsync();
        Assert.Equal(WaiverRequestStatus.Rejected, stored.Status);
        Assert.Null(stored.ApprovedAmount);
    }

    [Fact]
    public async Task Second_decision_with_stale_version_is_a_concurrency_failure()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, ScopedBranch);
        Assert.True((await Approve(db, Delegate(), waiver.Id, 100_000, 0)).IsSucceeded);

        var stale = await Reject(db, Delegate(), waiver.Id, 0, null);

        Assert.True(stale.IsFailed);
        Assert.Equal(WaiverErrors.Concurrency.Code, stale.Error!.Value.Code);
        Assert.Equal(WaiverRequestStatus.AwaitingAccountant,
            (await db.CustomerWaiverRequests.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Delegate_cannot_decide_outside_branch_scope()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, OtherBranch);

        var approve = await Approve(db, Delegate(), waiver.Id, 100_000, 0);
        var reject = await Reject(db, Delegate(), waiver.Id, 0, null);

        Assert.Equal(WaiverErrors.BranchDenied.Code, approve.Error!.Value.Code);
        Assert.Equal(WaiverErrors.BranchDenied.Code, reject.Error!.Value.Code);
        Assert.Equal(WaiverRequestStatus.Pending,
            (await db.CustomerWaiverRequests.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Decision_note_is_limited_to_500_chars()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, ScopedBranch);

        var result = await Reject(db, Delegate(), waiver.Id, 0, new string('م', 501));

        Assert.Equal(WaiverErrors.DecisionNoteTooLong.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task List_is_scoped_to_delegate_branches_and_counts_ignore_status_filter()
    {
        await using var db = CreateDb();
        var a = await SeedWaiverAsync(db, ScopedBranch);
        var b = await SeedWaiverAsync(db, ScopedBranch);
        await SeedWaiverAsync(db, OtherBranch);
        Assert.True((await Approve(db, Delegate(), a.Id, 10_000, 0)).IsSucceeded);
        Assert.True((await Reject(db, Delegate(), b.Id, 0, null)).IsSucceeded);
        await SeedWaiverAsync(db, ScopedBranch);

        var handler = new ListWaiversHandler(db, Delegate());
        var all = await handler.Handle(new ListWaiversQuery(), CancellationToken.None);
        var pendingOnly = await handler.Handle(new ListWaiversQuery { Status = "Pending" }, CancellationToken.None);
        var outside = await handler.Handle(new ListWaiversQuery { BranchId = OtherBranch }, CancellationToken.None);

        Assert.Equal(3, all.Data!.TotalCount);
        Assert.All(all.Data.Items, i => Assert.Equal(ScopedBranch, i.BranchId));
        Assert.Equal(1, pendingOnly.Data!.TotalCount);
        Assert.Equal(new WaiverStatusCountsDto(1, 1, 0, 1), pendingOnly.Data.Counts);
        Assert.Equal(1, pendingOnly.Data.AwaitingAccountantCount);
        Assert.Equal(0, outside.Data!.TotalCount);
        Assert.Empty(outside.Data.Items);
    }

    [Fact]
    public async Task Detail_outside_scope_is_denied()
    {
        await using var db = CreateDb();
        var waiver = await SeedWaiverAsync(db, OtherBranch);

        var result = await new GetWaiverByIdHandler(db, Delegate())
            .Handle(new GetWaiverByIdQuery { Id = waiver.Id }, CancellationToken.None);

        Assert.Equal(WaiverErrors.BranchDenied.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Detail_account_follows_customer_balance_rules_and_lists_related()
    {
        await using var db = CreateDb();
        var customer = NewCustomer("زبون التنازل", "07700000001");
        var twin = NewCustomer("زبون آخر", "07700000001");
        var stranger = NewCustomer("بعيد", "07799999999");
        db.Customers.AddRange(customer, twin, stranger);
        await db.SaveChangesAsync();

        var approved = NewOrder(customer.Id, 1_000_000);
        approved.InstallmentPayments.Add(new InstallmentPayment(300_000, new DateOnly(2026, 10, 1)));
        var pending = NewOrder(customer.Id, 700_000);
        pending.ApprovalStatus = OrderApprovalStatus.Pending;
        db.Orders.AddRange(approved, pending, NewOrder(twin.Id, 200_000));
        db.InstallmentPayments.Add(new InstallmentPayment(100_400, new DateOnly(2026, 11, 1)) { CustomerId = customer.Id });
        await db.SaveChangesAsync();

        var waiver = new CustomerWaiverRequest(customer.Id, ScopedBranch, approved.Id, RequesterId, "مدير فرع", "طلب", DateTimeOffset.UtcNow);
        db.CustomerWaiverRequests.Add(waiver);
        await db.SaveChangesAsync();

        var result = await new GetWaiverByIdHandler(db, Delegate())
            .Handle(new GetWaiverByIdQuery { Id = waiver.Id }, CancellationToken.None);

        Assert.True(result.IsSucceeded);
        var detail = result.Data!;
        Assert.Equal(700_000, detail.OrderRemaining);
        Assert.Equal(1_000_000, detail.Account!.CustomerSellTotal);
        Assert.Equal(400_400, detail.Account.CustomerPaidTotal);
        Assert.Equal(600_000, detail.Account.CustomerRemainingRounded);
        Assert.Equal(2, detail.RelatedOrders!.Count);
        Assert.Single(detail.RelatedOrders, o => o.IsWaiverOrder && o.OrderId == approved.Id);
        var match = Assert.Single(detail.RelatedCustomers!);
        Assert.Equal(twin.Id, match.CustomerId);
        Assert.Contains("نفس رقم الهاتف", match.MatchReasons);
        Assert.Equal(200_000, match.RemainingRounded);
    }

    private static Task<Result<WaiverDetailDto>> Approve(
        TestApplicationDbContext db, CurrentUserServiceStub user, int id, double amount, int version, string? note = null)
        => new ApproveWaiverByDelegateHandler(db, user, new DateTimeProviderStub())
            .Handle(new ApproveWaiverByDelegateCommand
            {
                Id = id,
                ApprovedAmount = amount,
                ExpectedVersion = version,
                Note = note
            }, CancellationToken.None);

    private static Task<Result<WaiverDetailDto>> Reject(
        TestApplicationDbContext db, CurrentUserServiceStub user, int id, int version, string? note)
        => new RejectWaiverHandler(db, user, new DateTimeProviderStub())
            .Handle(new RejectWaiverCommand { Id = id, ExpectedVersion = version, Note = note }, CancellationToken.None);

    private static async Task<CustomerWaiverRequest> SeedWaiverAsync(TestApplicationDbContext db, int branchId)
    {
        var customer = NewCustomer("زبون " + Guid.NewGuid().ToString("N")[..4], "0770" + Random.Shared.Next(1000000, 9999999), branchId);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        var waiver = new CustomerWaiverRequest(customer.Id, branchId, null, RequesterId, "مدير فرع", "طلب تنازل", DateTimeOffset.UtcNow);
        db.CustomerWaiverRequests.Add(waiver);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return waiver;
    }

    private static CurrentUserServiceStub Delegate()
        => new()
        {
            UserId = DelegateId,
            BranchIds = [ScopedBranch],
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CustomClaims.Permissions, string.Join(',', SeedPermissions.Waiver.Read, SeedPermissions.Waiver.Decide))],
                "test"))
        };

    private static Customer NewCustomer(string name, string phone, int branchId = ScopedBranch)
        => new(name, "ام", Guid.NewGuid().ToString("N")[..8], new DateOnly(1990, 1, 1), phone, phone,
            new Business("محل", "عنوان", "علامة"), branchId: branchId);

    private static Order NewOrder(int customerId, double sell)
        => Order.CreateByAdmin(sell * 0.6, sell, 0, 10_000, customerId, branchId: ScopedBranch, orderItems: []);

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options);
}
