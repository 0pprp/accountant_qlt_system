using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Common;
using Application.Safes.Common;
using Application.SalesRequests.Commands.App.Checkout;
using Application.SalesRequests.Commands.App.EnsureDraft;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProductAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Domain.Entities.SalesEmployeeAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Domain.Entities.WarehouseAggregate;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

/// <summary>
/// «تم البيع» على مزوّد علائقي حقيقي: InMemory يتجاهل المعاملات فلا يكشف المعاملة المتداخلة.
/// </summary>
public class CheckoutSaleSqliteTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task Checkout_completes_sale_posts_prepayment_to_list_safe_and_saves_shop()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedAsync(connection, stock: 3);
        await using var db = Db(connection);

        var result = await Checkout(db, seed, qty: 1);

        Assert.True(result.IsSucceeded, result.Error?.Message);
        var order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == seed.OrderId);
        Assert.Equal(seed.OrderListId, order.OrderListId);
        Assert.Equal(2, (await db.Products.AsNoTracking().SingleAsync(p => p.Id == seed.ProductId)).RemainingCount);
        var prepayment = await db.InstallmentPayments.AsNoTracking().SingleAsync(p => p.OrderId == seed.OrderId);
        Assert.Equal(170000, prepayment.Amount);
        Assert.Equal(PaymentSafePosting.PrepaymentNote, prepayment.Description);
        var listSafe = await db.Safes.AsNoTracking().SingleAsync(s => s.OrderListId == seed.OrderListId);
        Assert.Equal(170000, listSafe.Balance);
        var movement = await db.SafeMovements.AsNoTracking().SingleAsync(m => m.SafeId == listSafe.Id);
        Assert.Equal(SafeMovementKind.Collection, movement.Kind);
        Assert.Equal(PaymentSafePosting.PrepaymentNote, movement.Note);
        Assert.Equal(prepayment.Id, movement.InstallmentPaymentId);
        var shop = await db.CustomerShopProfiles.AsNoTracking().SingleAsync(s => s.OrderId == seed.OrderId);
        Assert.Equal(3000000, shop.StockEstimatedValue);
        Assert.Equal(50000, shop.EstimatedDailyRevenue);
        Assert.Equal(20, shop.ShopArea);
        Assert.Equal("مقهى", shop.BusinessType);
        var request = await db.SalesRequests.AsNoTracking().SingleAsync(r => r.Id == seed.RequestId);
        Assert.Equal(SalesRequestStatus.Completed, request.Status);
    }

    [Fact]
    public async Task Checkout_more_than_stock_is_a_clear_refusal_not_a_server_error()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedAsync(connection, stock: 3);
        await using var db = Db(connection);

        var result = await Checkout(db, seed, qty: 26);

        Assert.True(result.IsFailed);
        Assert.Equal(OrderErrors.ThereIsNotEnoughQuantityOfThisProduct.Code, result.Error!.Value.Code);
        Assert.Equal(3, (await db.Products.AsNoTracking().SingleAsync(p => p.Id == seed.ProductId)).RemainingCount);
    }

    private static async Task<Result<int>> Checkout(TestApplicationDbContext db, Seed seed, int qty)
    {
        var user = Actor(seed.SellerId, seed.BranchId);
        var services = new ServiceCollection();
        services.AddSingleton<IApplicationDbContext>(db);
        services.AddSingleton<ICurrentUserService>(user);
        services.AddSingleton<INotificationService>(new NotificationServiceStub());
        services.AddSingleton<IActivityLogService>(new ActivityLogServiceStub());
        services.AddSingleton<ISafeLedger>(new SafeLedger(db, user));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ChangeOrderApprovalStatusHandler>());
        await using var provider = services.BuildServiceProvider();

        var handler = new RequestCheckout(db, user, new Clock(), provider.GetRequiredService<IMediator>());
        return await handler.Handle(new CheckoutSale(seed.OrderId, new CheckoutBody(
            seed.OrderListId,
            [new SaleLine(seed.ProductId, qty)],
            3000000m * qty,
            20000m,
            170000m,
            "النجف — الكوفة",
            32.0226967,
            44.3801035,
            ShopName: "مقهى الساعدي",
            BusinessType: "مقهى",
            GoodsValue: 3000000,
            DailyIncome: 50000,
            ShopLength: 5,
            ShopWidth: 4)), default);
    }

    private static async Task<Seed> SeedAsync(SqliteConnection connection, int stock)
    {
        await using var db = Db(connection);
        await db.Database.EnsureCreatedAsync();

        var province = new Province("النجف");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();
        var branch = new Branch("الكوفة", province.Id);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var seller = NewUser("موظف مبيعات", "seller", branch.Id);
        var mandob = NewUser("مندوب الكوفة", "mandob", branch.Id);
        var motaba = NewUser("متابع الكوفة", "motaba", branch.Id);
        db.Users.AddRange(seller, mandob, motaba);
        await db.SaveChangesAsync();
        db.SalesEmployeeAssignments.Add(new SalesEmployeeAssignment(seller.Id, branch.Id, Now.AddDays(-1), null));

        var list = new OrderList("الكوفة", mandob.Id, motaba.Id, branch.Id);
        db.OrderLists.Add(list);
        var category = new ProductCategory("موبايلات");
        db.ProductCategories.Add(category);
        var warehouse = new Warehouse("مخزن الكوفة", branch.Id);
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync();
        var product = new Product("ايفون 17 برو ماكس", stock, 2500000, 3000000, 20000, null, category.Id)
        {
            WarehouseId = warehouse.Id
        };
        db.Products.Add(product);

        var customer = new Customer("نعيم عبد راضي", "أم", "1", new DateOnly(1990, 1, 1), "07884961542", "07884961542",
            new Business("مقهى الساعدي", "الكوفة", "قرب الجسر"), branch.Id);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var request = new SalesRequest("نعيم عبد راضي", "07884961542", SalesRequestSourceType.Manual, branch.Id, null, null, null);
        request.Assign(seller.Id, Now.AddHours(-2));
        request.ExistingCustomerId = customer.Id;
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var draft = await new EnsureSalesRequestDraftHandler(
                db, Actor(seller.Id, branch.Id, SeedPermissions.SalesRequest.AppLinkDraft), new Clock())
            .Handle(new EnsureSalesRequestDraftCommand(request.Id), default);
        Assert.True(draft.IsSucceeded, draft.Error?.Message);

        return new Seed(request.Id, draft.Data!.LinkedOrderId!.Value, seller.Id, list.Id, product.Id, branch.Id);
    }

    private static User NewUser(string name, string tag, int branchId)
    {
        var u = new User(name, "أم", "n", new DateOnly(1990, 1, 1), $"{tag}-{Guid.NewGuid():N}", "hash", "عنوان", "0780")
        {
            CreationStep = UserCreationStep.Completed
        };
        u.UserBranches.Add(new UserBranch(branchId));
        return u;
    }

    private static TestApplicationDbContext Db(SqliteConnection connection) =>
        new SqliteDb(new DbContextOptionsBuilder<TestApplicationDbContext>().UseSqlite(connection).Options);

    private sealed class SqliteDb(DbContextOptions<TestApplicationDbContext> options) : TestApplicationDbContext(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<DateTimeOffset>()
                .HaveConversion<Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter>();
        }
    }

    private static CurrentUserServiceStub Actor(int userId, int branchId = 1, params string[] permissions) => new()
    {
        UserId = userId,
        BranchIds = [branchId],
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permissions", string.Join(",", permissions))], "test"))
    };


    private sealed class Clock : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => Now;
    }

    private sealed record Seed(int RequestId, int OrderId, int SellerId, int OrderListId, int ProductId, int BranchId);
}
