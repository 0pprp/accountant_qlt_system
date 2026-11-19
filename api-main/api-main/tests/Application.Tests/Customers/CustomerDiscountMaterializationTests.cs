using Application.Common.Models;
using Application.Common.Interfaces;
using Application.Customers.Common;
using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Application.Tests.Customers;

public class CustomerDiscountMaterializationTests
{
    [Fact]
    public async Task Discount_AppliesInFull_ToEveryOrderIndependently()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000), (900_000, 5_000)]);

        customer.SetDiscounts(100_000, 1_000);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsSucceeded);
        var orders = await db.Orders.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(1_400_000, orders[0].SellAmount);
        Assert.Equal(9_000, orders[0].DailyInstallmentAmount);
        Assert.Equal(800_000, orders[1].SellAmount);
        Assert.Equal(4_000, orders[1].DailyInstallmentAmount);
    }

    [Fact]
    public async Task SavingTheSameDiscountTwice_IsAByteIdenticalNoOp()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(100_000, 1_000);
        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        var order = await db.Orders.SingleAsync();
        Assert.Equal(1_400_000, order.SellAmount);
        Assert.Equal(100_000, order.SellAmountDiscountApplied);
        Assert.Equal(9_000, order.DailyInstallmentAmount);
    }

    [Fact]
    public async Task RaisingTheDiscount_AppliesOnlyTheDelta()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(10_000, 0);
        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        customer.SetDiscounts(20_000, 0);
        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        var order = await db.Orders.SingleAsync();
        Assert.Equal(1_480_000, order.SellAmount);
        Assert.Equal(20_000, order.SellAmountDiscountApplied);
    }

    [Fact]
    public async Task ClearingTheDiscount_RestoresTheOriginalAmountsExactly()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(20_000, 2_500);
        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        customer.SetDiscounts(0, 0);
        await Reconcile(db, customer);
        await db.SaveChangesAsync();

        var order = await db.Orders.SingleAsync();
        Assert.Equal(1_500_000, order.SellAmount);
        Assert.Equal(10_000, order.DailyInstallmentAmount);
        Assert.Equal(0, order.SellAmountDiscountApplied);
        Assert.Equal(0, order.DailyInstallmentDiscountApplied);
    }

    [Fact]
    public async Task DiscountBelowWhatIsAlreadyPaid_IsRejected_AndNoOrderIsTouched()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000), (900_000, 5_000)]);
        var first = await db.Orders.OrderBy(x => x.Id).FirstAsync();
        first.InstallmentPayments.Add(new InstallmentPayment(1_400_000, DateOnly.FromDateTime(DateTime.UtcNow)));
        await db.SaveChangesAsync();

        customer.SetDiscounts(200_000, 0);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsFailed);
        Assert.Equal(CustomerErrors.CustomerDiscountExceedsOrderRemainingAmount, result.Error);

        var orders = await db.Orders.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(1_500_000, orders[0].SellAmount);
        Assert.Equal(900_000, orders[1].SellAmount);
    }

    [Fact]
    public async Task DiscountThatReachesTheDailyInstallment_IsRejected()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(0, 10_000);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsFailed);
        Assert.Equal(CustomerErrors.CustomerDiscountExceedsOrderDailyInstallment, result.Error);
    }

    [Fact]
    public async Task DiscountEqualToTheSellAmount_IsRejected()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(1_500_000, 0);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsFailed);
        Assert.Equal(CustomerErrors.CustomerDiscountExceedsOrderSellAmount, result.Error);
    }

    [Fact]
    public async Task ZeroAmountDraftOrder_IsSkipped_AndDoesNotBlockTheSave()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(0, 0), (1_500_000, 10_000)]);

        customer.SetDiscounts(50_000, 1_000);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsSucceeded);
        var orders = await db.Orders.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(0, orders[0].SellAmount);
        Assert.Equal(0, orders[0].SellAmountDiscountApplied);
        Assert.Equal(1_450_000, orders[1].SellAmount);
    }

    [Fact]
    public async Task DiscountThatClosesTheDebt_MarksTheOrderCompleted()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);
        var order = await db.Orders.SingleAsync();
        order.InstallmentPayments.Add(new InstallmentPayment(1_400_000, DateOnly.FromDateTime(DateTime.UtcNow)));
        await db.SaveChangesAsync();

        customer.SetDiscounts(100_000, 0);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsSucceeded);
        var reloaded = await db.Orders.SingleAsync();
        Assert.Equal(1_400_000, reloaded.SellAmount);
        Assert.Equal(OrderExecutionStatus.Completed, reloaded.ExecutionStatus);
    }

    [Fact]
    public async Task LoweringADiscountOnACompletedOrder_IsRejected()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)]);

        customer.SetDiscounts(100_000, 0);
        await Reconcile(db, customer);
        var order = await db.Orders.SingleAsync();
        order.ExecutionStatus = OrderExecutionStatus.Completed;
        await db.SaveChangesAsync();

        customer.SetDiscounts(0, 0);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsFailed);
        Assert.Equal(CustomerErrors.CustomerDiscountCanNotRaiseCompletedOrderAmount, result.Error);
    }

    [Fact]
    public async Task PrepaymentActsAsAFloor_BecauseApprovalTurnsItIntoAPayment()
    {
        await using var db = CreateDb();
        var customer = await SeedAsync(db, orders: [(1_500_000, 10_000)], prepayment: 1_450_000);

        customer.SetDiscounts(100_000, 0);
        var result = await Reconcile(db, customer);

        Assert.True(result.IsFailed);
        Assert.Equal(CustomerErrors.CustomerDiscountExceedsOrderRemainingAmount, result.Error);
    }

    private static Task<Result> Reconcile(TestApplicationDbContext db, Customer customer) =>
        CustomerDiscountApplier.ReconcileCustomerOrdersAsync(
            db, customer, new ActivityLogServiceStub(), CancellationToken.None);

    private static TestApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static async Task<Customer> SeedAsync(
        TestApplicationDbContext db,
        (double Sell, double Daily)[] orders,
        double prepayment = 0)
    {
        var customer = new Customer(
            "حسين الجبوري",
            "أم حسين",
            "19900001",
            new DateOnly(1990, 1, 1),
            "07701234567",
            "07701234567",
            new Business("بقالة", "النجف", "قرب السوق"),
            branchId: 1);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        foreach (var (sell, daily) in orders)
        {
            var order = Order.CreateByAdmin(
                sell * 0.6,
                sell,
                prepayment,
                daily,
                customer.Id,
                branchId: 1,
                orderItems: []);

            db.Orders.Add(order);
        }

        await db.SaveChangesAsync();
        return customer;
    }

    private sealed class ActivityLogServiceStub : IActivityLogService
    {
        public Task AddAsync(
            int branchId,
            ActivityType activityType,
            TargetEntityType targetEntityType,
            int? targetEntityId,
            ActivityLogActor? actorOverride = null,
            string? details = null) => Task.CompletedTask;
    }
}
