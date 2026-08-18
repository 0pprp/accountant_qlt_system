using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Common;
using Application.Purchases.Queries.Admin.GetLastFactorNumber;
using Application.Tests;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.ProductAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Orders;

public class ChangeOrderApprovalStatusHandlerTests
{
    [Fact]
    public async Task Approve_WhenPurchasesTableIsEmpty_AndOrderHasForeignProduct_SucceedsAndCreatesFirstPurchase()
    {
        await using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, includeForeignProduct: true);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new ChangeOrderApprovalStatusCommand
            {
                OrderId = seed.OrderId,
                ApprovalStatus = OrderApprovalStatus.Approved
            },
            CancellationToken.None);

        Assert.True(result.IsSucceeded);
        var purchase = Assert.Single(await dbContext.Purchases.Include(x => x.PurchaseItems).ToListAsync());
        Assert.Equal(1, purchase.FactorNumber);
        Assert.Equal(seed.OrderId, purchase.OrderId);
        Assert.Equal("منتج خارجي", Assert.Single(purchase.PurchaseItems).ForeignProductName);

        var order = await dbContext.Orders.SingleAsync(x => x.Id == seed.OrderId);
        Assert.Equal(OrderApprovalStatus.Approved, order.ApprovalStatus);
    }

    [Fact]
    public async Task Approve_WhenPreviousPurchasesExist_CreatesPurchaseWithNextFactorNumber()
    {
        await using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, includeForeignProduct: true);

        var existingTransaction = new Transaction(50, TransactionType.Purchase, TransactionStatus.Approved, TransactionDirection.Out, seed.SafeId);
        dbContext.Purchases.Add(new Purchase(
            5,
            50,
            SafeType.Branch,
            seed.BranchId,
            existingTransaction,
            [new PurchaseItem(1, 50, "شراء سابق")]));
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new ChangeOrderApprovalStatusCommand
            {
                OrderId = seed.OrderId,
                ApprovalStatus = OrderApprovalStatus.Approved
            },
            CancellationToken.None);

        Assert.True(result.IsSucceeded);
        var createdPurchase = await dbContext.Purchases.SingleAsync(x => x.OrderId == seed.OrderId);
        Assert.Equal(6, createdPurchase.FactorNumber);
        Assert.Equal(2, await dbContext.Purchases.CountAsync());
    }

    [Fact]
    public async Task Approve_WhenOrderHasNoForeignProducts_DoesNotCreatePurchase()
    {
        await using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, includeForeignProduct: false, includeWarehouseProduct: true);

        var remainingCountBefore = (await dbContext.Products.SingleAsync()).RemainingCount;

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new ChangeOrderApprovalStatusCommand
            {
                OrderId = seed.OrderId,
                ApprovalStatus = OrderApprovalStatus.Approved
            },
            CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Empty(await dbContext.Purchases.ToListAsync());

        var order = await dbContext.Orders.SingleAsync(x => x.Id == seed.OrderId);
        Assert.Equal(OrderApprovalStatus.Approved, order.ApprovalStatus);

        var remainingCountAfter = (await dbContext.Products.SingleAsync()).RemainingCount;
        Assert.Equal(remainingCountBefore - 2, remainingCountAfter);
    }

    [Fact]
    public async Task Approve_WhenOrderHasMultipleForeignProducts_CreatesOnePurchaseWithAllItems()
    {
        await using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, includeForeignProduct: true, extraForeignProduct: true);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new ChangeOrderApprovalStatusCommand
            {
                OrderId = seed.OrderId,
                ApprovalStatus = OrderApprovalStatus.Approved
            },
            CancellationToken.None);

        Assert.True(result.IsSucceeded);
        var purchase = Assert.Single(await dbContext.Purchases.Include(x => x.PurchaseItems).ToListAsync());
        Assert.Equal(2, purchase.PurchaseItems.Count);
        Assert.Equal(300, purchase.TotalAmount);
        Assert.Contains(purchase.PurchaseItems, x => x.ForeignProductName == "منتج خارجي");
        Assert.Contains(purchase.PurchaseItems, x => x.ForeignProductName == "منتج خارجي 2");
    }

    [Fact]
    public async Task Approve_WhenOrderAlreadyApproved_ReturnsApprovalStatusAlreadySet_AndDoesNotCreateAnotherPurchase()
    {
        await using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, includeForeignProduct: true);

        var handler = CreateHandler(dbContext);
        var command = new ChangeOrderApprovalStatusCommand
        {
            OrderId = seed.OrderId,
            ApprovalStatus = OrderApprovalStatus.Approved
        };

        var firstResult = await handler.Handle(command, CancellationToken.None);
        Assert.True(firstResult.IsSucceeded);
        Assert.Equal(1, await dbContext.Purchases.CountAsync());

        var secondResult = await handler.Handle(command, CancellationToken.None);
        Assert.True(secondResult.IsFailed);
        Assert.Equal(OrderErrors.ApprovalStatusOfOrderHasBeenSetBefore, secondResult.Error);
        Assert.Equal(1, await dbContext.Purchases.CountAsync());
    }

    [Fact]
    public async Task Approve_WhenOrderDoesNotExist_ReturnsOrderNotFound()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, includeForeignProduct: true);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new ChangeOrderApprovalStatusCommand
            {
                OrderId = 999,
                ApprovalStatus = OrderApprovalStatus.Approved
            },
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(OrderErrors.OrderNotFound, result.Error);
        Assert.Empty(await dbContext.Purchases.ToListAsync());
    }

    [Fact]
    public async Task GetLastPurchaseFactorNumber_WhenPurchasesTableIsEmpty_ReturnsZero()
    {
        await using var dbContext = CreateDbContext();
        var handler = new GetLastPurchaseFactorNumberHandler(dbContext);

        var result = await handler.Handle(new GetLastPurchaseFactorNumberQuery(), CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal(0, result.Data!.LastFactorNumber);
    }

    private static ChangeOrderApprovalStatusHandler CreateHandler(TestApplicationDbContext dbContext)
    {
        return new ChangeOrderApprovalStatusHandler(dbContext, new NotificationServiceStub(), new ActivityLogServiceStub());
    }

    private static TestApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static async Task<SeedResult> SeedAsync(
        TestApplicationDbContext dbContext,
        bool includeForeignProduct,
        bool includeWarehouseProduct = false,
        bool extraForeignProduct = false)
    {
        var province = new Province("بغداد");
        dbContext.Provinces.Add(province);
        await dbContext.SaveChangesAsync();

        var branch = new Branch("الفرع الرئيسي", province.Id);
        branch.Safe!.RemainingCashAmount = 100000;
        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync();

        var seller = new User(
            "بائع",
            "أم البائع",
            "1234567890",
            new DateOnly(1990, 1, 1),
            "seller",
            "hash",
            "العنوان",
            "07700000000");
        dbContext.Users.Add(seller);

        var customer = new Customer(
            "زبون",
            "أم الزبون",
            "9876543210",
            new DateOnly(1985, 5, 5),
            "07711111111",
            "07711111111",
            new Business("محل", "العنوان", "علامة"),
            branch.Id);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var orderItems = new List<OrderItem>();
        var buyAmount = 0d;
        var sellAmount = 0d;

        if (includeWarehouseProduct)
        {
            var category = new ProductCategory("تصنيف");
            dbContext.ProductCategories.Add(category);
            await dbContext.SaveChangesAsync();

            var product = new Product("منتج مخزن", 10, 80, 120, 10, null, category.Id);
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            orderItems.Add(new OrderItem(
                ProductType.Warehouse,
                2,
                160,
                240,
                0,
                20,
                product.Name,
                product.Id));
            buyAmount += 160;
            sellAmount += 240;
        }

        if (includeForeignProduct)
        {
            orderItems.Add(new OrderItem(
                ProductType.Foreign,
                1,
                100,
                150,
                0,
                10,
                "منتج خارجي",
                null));
            buyAmount += 100;
            sellAmount += 150;
        }

        if (extraForeignProduct)
        {
            orderItems.Add(new OrderItem(
                ProductType.Foreign,
                2,
                200,
                280,
                0,
                20,
                "منتج خارجي 2",
                null));
            buyAmount += 200;
            sellAmount += 280;
        }

        var order = Order.CreateByAppUser(
            buyAmount,
            sellAmount,
            0,
            10,
            customer.Id,
            branch.Id,
            seller.Id,
            orderItems);
        order.Step = OrderStep.Completed;
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        return new SeedResult(order.Id, branch.Id, branch.Safe!.Id);
    }

    private sealed record SeedResult(int OrderId, int BranchId, int SafeId);
}
