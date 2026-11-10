using Application.InstallmentPayments.Common;
using Application.Orders.Commands.Admin.SetSellerInfo;
using Application.Orders.Common;
using Application.Tests;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.InstallmentPayments;

public class ClientPaymentIdSafetyTests
{
    [Fact]
    public async Task SameKey_SameFingerprint_ReturnsAlreadyApplied_WithoutSecondCharge()
    {
        await using var db = CreateDb();
        var seed = await SeedReadyAsync(db, sellAmount: 100000);
        var writer = Writer(db);
        var key = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 19);

        var first = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);
        var second = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);

        Assert.True(first.IsSucceeded);
        Assert.False(first.Data!.AlreadyApplied);
        Assert.True(second.IsSucceeded);
        Assert.True(second.Data!.AlreadyApplied);
        Assert.Equal(first.Data.PaymentId, second.Data.PaymentId);
        Assert.Equal(1, await db.InstallmentPayments.CountAsync(x => x.ClientPaymentId == key));
    }

    [Fact]
    public async Task SameKey_DifferentAmount_ReturnsConflict()
    {
        await using var db = CreateDb();
        var seed = await SeedReadyAsync(db, sellAmount: 100000);
        var writer = Writer(db);
        var key = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 19);

        Assert.True((await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None)).IsSucceeded);

        var conflict = await writer.WriteAsync(seed.MandobId, seed.OrderId, 2000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);

        Assert.True(conflict.IsFailed);
        Assert.Equal(OrderErrors.ClientPaymentPayloadConflict, conflict.Error);
        Assert.Equal(1, await db.InstallmentPayments.CountAsync(x => x.ClientPaymentId == key));
    }

    [Fact]
    public async Task SameKey_OtherAccount_ReturnsNotFound_WithoutLeak()
    {
        await using var db = CreateDb();
        var seed = await SeedReadyAsync(db, sellAmount: 100000);
        var writer = Writer(db);
        var key = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 19);

        Assert.True((await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None)).IsSucceeded);

        var other = await writer.WriteAsync(seed.MotabaId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);

        Assert.True(other.IsFailed);
        Assert.Equal(OrderErrors.OrderNotFound, other.Error);
    }

    [Fact]
    public async Task RetryAfterOrderCompleted_AcksSamePayment_RejectsNewKey()
    {
        await using var db = CreateDb();
        var seed = await SeedReadyAsync(db, sellAmount: 1000);
        var writer = Writer(db);
        var key = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 19);

        var first = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);
        Assert.True(first.IsSucceeded);
        Assert.Equal(OrderExecutionStatus.Completed,
            (await db.Orders.SingleAsync(x => x.Id == seed.OrderId)).ExecutionStatus);

        var ack = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, key,
            ActivityType.InstallmentPaymentsSynced, CancellationToken.None);
        Assert.True(ack.IsSucceeded);
        Assert.True(ack.Data!.AlreadyApplied);

        var fresh = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, Guid.NewGuid(),
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);
        Assert.True(fresh.IsFailed);
        Assert.Equal(OrderErrors.OrderNotFound, fresh.Error);
    }

    [Fact]
    public void AdvisoryLockNamespaces_AreStableInts_DistinctFromEnsureDraft()
    {
        // ensure-draft = 791901؛ submission lock أحادي المفتاح — لا GetHashCode.
        Assert.Equal(791902, AppInstallmentPaymentWriter.OrderLockNamespace);
        Assert.Equal(791903, AppInstallmentPaymentWriter.CashHolderLockNamespace);
        Assert.NotEqual(791901, AppInstallmentPaymentWriter.OrderLockNamespace);
        Assert.NotEqual(
            AppInstallmentPaymentWriter.OrderLockNamespace,
            AppInstallmentPaymentWriter.CashHolderLockNamespace);
    }

    [Fact]
    public async Task TwoKeys_ExceedingRemaining_SecondFails_FirstStands()
    {
        await using var db = CreateDb();
        var seed = await SeedReadyAsync(db, sellAmount: 1500);
        var writer = Writer(db);
        var date = new DateOnly(2026, 8, 19);

        Assert.True((await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, Guid.NewGuid(),
            ActivityType.InstallmentPaymentCreated, CancellationToken.None)).IsSucceeded);
        var second = await writer.WriteAsync(seed.MandobId, seed.OrderId, 1000, date, Guid.NewGuid(),
            ActivityType.InstallmentPaymentCreated, CancellationToken.None);

        Assert.True(second.IsFailed);
        Assert.Equal(OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount, second.Error);
        Assert.Equal(1, await db.InstallmentPayments.CountAsync(x => x.Amount == 1000 && x.ClientPaymentId != null));
    }

    private static AppInstallmentPaymentWriter Writer(TestApplicationDbContext db) =>
        new(db, new DateTimeProviderStub(), new ActivityLogServiceStub());

    private static TestApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TestApplicationDbContext(options);
    }

    private static async Task<(int OrderId, int MandobId, int MotabaId)> SeedReadyAsync(
        TestApplicationDbContext db,
        double sellAmount)
    {
        var province = new Province("النجف");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var branch = new Branch("الشمالية", province.Id);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var mandobRole = new Role(SeedRoles.Mandob.Name, SeedRoles.Mandob.DisplayName);
        var motabaRole = new Role(SeedRoles.Motaba.Name, SeedRoles.Motaba.DisplayName);
        db.Roles.AddRange(mandobRole, motabaRole);
        await db.SaveChangesAsync();

        var mandob = new User(
            "مندوب", "الأم", "m1234567890", new DateOnly(1990, 1, 1),
            "mandob_t", "hash", "عنوان", "07700000001");
        var motaba = new User(
            "متابع", "الأم", "t1234567890", new DateOnly(1990, 1, 1),
            "motaba_t", "hash", "عنوان", "07700000002");
        db.Users.AddRange(mandob, motaba);
        await db.SaveChangesAsync();
        db.UserRoles.AddRange(new UserRole(mandob.Id, mandobRole.Id), new UserRole(motaba.Id, motabaRole.Id));

        var list = new OrderList("قائمة", mandob.Id, motaba.Id, branch.Id);
        db.OrderLists.Add(list);

        var customer = new Customer(
            "زبون", "أم", "1234567890", new DateOnly(1985, 1, 1),
            "07711111111", "07711111111",
            new Business("محل", "العنوان", "علامة"),
            branch.Id);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var order = Order.CreateByAdmin(
            0, sellAmount, 0, 1000, customer.Id, branch.Id,
            [new OrderItem(ProductType.Foreign, 1, 0, sellAmount, 0, 1000, "منتج", null)]);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var setSeller = new SetOrderSellerInfoHandler(
            db,
            new CurrentUserServiceStub { Role = SeedRoles.BranchAccountant.Name, BranchIds = [branch.Id] },
            new DateTimeProviderStub(),
            new NotificationServiceStub(),
            new ActivityLogServiceStub());
        var ok = await setSeller.Handle(new SetOrderSellerInfoCommand
        {
            OrderId = order.Id,
            CreationAddress = "نجف",
            SaleDate = new DateOnly(2026, 8, 19),
            SaleTime = new TimeOnly(11, 0),
            SellerId = mandob.Id,
            OrderListId = list.Id
        }, CancellationToken.None);
        Assert.True(ok.IsSucceeded);

        // ابدأ من رصيد نقد صفر لاختبارات المتبقي بدقة.
        mandob.UndeliveredCashAmount = 0;
        await db.SaveChangesAsync();
        // احذف دفعة العربون إن وُجدت حتى SellAmount الصافي متاح للتحصيل في الاختبار.
        var prepaid = await db.InstallmentPayments.Where(x => x.OrderId == order.Id).ToListAsync();
        db.InstallmentPayments.RemoveRange(prepaid);
        order.ExecutionStatus = OrderExecutionStatus.InProgress;
        await db.SaveChangesAsync();

        return (order.Id, mandob.Id, motaba.Id);
    }
}
