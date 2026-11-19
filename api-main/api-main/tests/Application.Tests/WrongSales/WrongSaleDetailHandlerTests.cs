using System.Security.Claims;
using Application.Common.Models.TokenFactory;
using Application.WrongSales.Queries;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.WrongSales;

public class WrongSaleDetailHandlerTests
{
    [Fact]
    public async Task Detail_requires_read_permission()
    {
        await using var db = CreateDb();
        var seeded = await Seed(db);

        var current = User(SeedPermissions.WrongSale.MarkDelegateRead, [seeded.BranchId]);
        var result = await new GetWrongSaleDetailHandler(db, current)
            .Handle(new GetWrongSaleDetailQuery { Id = seeded.NoticeId }, default);

        Assert.True(result.IsFailed);
        Assert.Equal("WrongSale.Forbidden", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Detail_outside_branch_scope_is_rejected()
    {
        await using var db = CreateDb();
        var seeded = await Seed(db);

        var current = User(SeedPermissions.WrongSale.Read, [seeded.BranchId + 1000]);
        var result = await new GetWrongSaleDetailHandler(db, current)
            .Handle(new GetWrongSaleDetailQuery { Id = seeded.NoticeId }, default);

        Assert.Equal("WrongSale.BranchScope", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Detail_with_empty_scope_is_rejected()
    {
        await using var db = CreateDb();
        var seeded = await Seed(db);

        var current = User(SeedPermissions.WrongSale.Read, []);
        var result = await new GetWrongSaleDetailHandler(db, current)
            .Handle(new GetWrongSaleDetailQuery { Id = seeded.NoticeId }, default);

        Assert.Equal("WrongSale.BranchScope", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Detail_missing_notice_returns_not_found()
    {
        await using var db = CreateDb();
        var seeded = await Seed(db);

        var current = User(SeedPermissions.WrongSale.Read, [seeded.BranchId]);
        var result = await new GetWrongSaleDetailHandler(db, current)
            .Handle(new GetWrongSaleDetailQuery { Id = 987654 }, default);

        Assert.Equal("WrongSale.NotFound", result.Error!.Value.Code);
    }

    [Fact(Skip = "InMemory provider cannot materialize the Customer.Business complex type")]
    public async Task Detail_returns_customer_sale_payments_evaluation_and_branch_matches()
    {
        await using var db = CreateDb();
        var seeded = await Seed(db);

        var current = User(SeedPermissions.WrongSale.Read, [seeded.BranchId]);
        var result = await new GetWrongSaleDetailHandler(db, current)
            .Handle(new GetWrongSaleDetailQuery { Id = seeded.NoticeId }, default);

        Assert.True(result.IsSucceeded);
        var d = result.Data!;
        Assert.Null(d.Review);
        Assert.Equal("فرع الاختبار", d.Customer.BranchName);
        Assert.Equal("قائمة أ", d.Customer.ListName);
        Assert.Equal("عنوان الزبون", d.Customer.Address);
        Assert.Equal(1_000_000m, d.Sale.SellAmount);
        Assert.Equal(10_000m, d.Sale.DailyInstallment);
        Assert.Equal(300_000m, d.Sale.PaidOnOrder);
        Assert.Equal(700_000m, d.Sale.RemainingOnOrder);
        Assert.Equal(600_000m, d.Sale.CustomerRemaining);
        Assert.Equal(2, d.PaymentsCount);
        Assert.Equal(2, d.Payments.Count);
        Assert.Equal("تقييم المتابع", d.Evaluation.Title);
        Assert.True(d.Evaluation.IsEvaluated);
        Assert.Equal(4, d.Evaluation.Rating);

        var match = Assert.Single(d.Matches);
        Assert.Equal(seeded.PhoneTwinId, match.CustomerId);
        Assert.Contains(WrongSaleDetailRules.PhoneReason, match.MatchReasons);
        Assert.DoesNotContain(d.Matches, m => m.CustomerId == seeded.OtherBranchTwinId);
    }

    [Fact]
    public void Mandob_source_uses_collection_delegate_title()
    {
        Assert.Equal("تقييم مندوب الجباية", WrongSaleDetailRules.EvaluationTitle(WrongSaleNotice.SourceMandobFirstCollection));
        Assert.Equal("تقييم المتابع", WrongSaleDetailRules.EvaluationTitle(WrongSaleNotice.SourceFollowerAuth));
    }

    [Fact]
    public async Task List_seen_mailbox_excludes_unread_and_reviewed_and_counts_each_chip()
    {
        await using var db = CreateDb();
        var unread = MakeNotice(1, 5);
        var seen = MakeNotice(2, 5);
        var reviewed = MakeNotice(3, 5);
        db.WrongSaleNotices.AddRange(unread, seen, reviewed);
        await db.SaveChangesAsync();
        seen.MarkDelegateRead(1, DateTimeOffset.UtcNow, 0);
        reviewed.MarkDelegateRead(1, DateTimeOffset.UtcNow, 0);
        reviewed.MarkBranchReviewed(2, "مراجعة الفرع", DateTimeOffset.UtcNow, 1);
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.WrongSale.Read, [5]);
        var result = await new ListWrongSalesHandler(db, current)
            .Handle(new ListWrongSalesQuery { Mailbox = "seen" }, default);

        Assert.True(result.IsSucceeded);
        var item = Assert.Single(result.Data!.Items);
        Assert.Equal(seen.Id, item.Id);
        Assert.Equal(1, result.Data.UnreadCount);
        Assert.Equal(1, result.Data.SeenCount);
        Assert.Equal(1, result.Data.ReviewedCount);
    }

    private sealed record Seeded(int BranchId, int NoticeId, int PhoneTwinId, int OtherBranchTwinId);

    private static async Task<Seeded> Seed(TestApplicationDbContext db)
    {
        var branch = new Branch("فرع الاختبار", 1);
        var otherBranch = new Branch("فرع آخر", 1);
        db.Branches.AddRange(branch, otherBranch);
        await db.SaveChangesAsync();

        var list = new OrderList("قائمة أ", 50, 60, branch.Id);
        db.OrderLists.Add(list);
        var customer = NewCustomer("علي حسن كاظم", "07701234567", branch.Id);
        var phoneTwin = NewCustomer("زيد عمر خالد", "009647701234567", branch.Id);
        var otherBranchTwin = NewCustomer("علي حسن كاظم", "07701234567", otherBranch.Id);
        var stranger = NewCustomer("سعد ماجد فهد", "07809999999", branch.Id);
        db.Customers.AddRange(customer, phoneTwin, otherBranchTwin, stranger);
        await db.SaveChangesAsync();

        var order = Order.CreateByAdmin(600_000, 1_000_000, 0, 10_000, customer.Id, branch.Id, []);
        order.OrderListId = list.Id;
        order.InstallmentPayments.Add(new InstallmentPayment(300_000, new DateOnly(2026, 10, 1)));
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        db.InstallmentPayments.Add(new InstallmentPayment(100_000, new DateOnly(2026, 10, 5)) { CustomerId = customer.Id });

        var notice = new WrongSaleNotice(order.Id, customer.Id, branch.Id, 9, "متابع أ", customer.FullName,
            customer.PhoneNumber, 4, "ملاحظة", 1000, 200, "مبيعات خاطئة", DateTimeOffset.UtcNow);
        db.WrongSaleNotices.Add(notice);
        await db.SaveChangesAsync();

        return new Seeded(branch.Id, notice.Id, phoneTwin.Id, otherBranchTwin.Id);
    }

    private static Customer NewCustomer(string name, string phone, int branchId)
        => new(name, "ام", Guid.NewGuid().ToString("N")[..8], new DateOnly(1990, 1, 1), phone, phone,
            new Business("محل", "عنوان الزبون", "علامة"), branchId);

    private static WrongSaleNotice MakeNotice(int orderId, int branchId) =>
        new(orderId, 100 + orderId, branchId, 9, "متابع", "زبون", "07", 4, "ملاحظة",
            1000, 200, "مبيعات خاطئة", DateTimeOffset.UtcNow);

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
