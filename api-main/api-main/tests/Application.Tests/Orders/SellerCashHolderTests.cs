using Application.Common.Models;
using Application.InstallmentPayments.Commands.Admin.Create;
using Application.InstallmentPayments.Commands.Admin.Delete;
using Application.InstallmentPayments.Commands.Admin.Update;
using Application.InstallmentPayments.Commands.App.Create;
using Application.InstallmentPayments.Commands.App.Sync;
using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Commands.Admin.SetSellerInfo;
using Application.Orders.Commands.Admin.Update;
using Application.Orders.Common;
using Application.Safes.Commands.Admin.CreateCashDelivery;
using Application.Safes.Queries.Admin.GetByBranch;
using Application.Safes.Queries.Admin.GetSellers;
using Application.Tests;
using Application.Transactions.Commands.Admin.ChangeStatus;
using Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using AdminCreateInstallment = Application.InstallmentPayments.Commands.Admin.Create.CreateInstallmentPaymentCommand;
using AdminCreateInstallmentHandler = Application.InstallmentPayments.Commands.Admin.Create.CreateInstallmentPaymentHandler;
using AppCreateInstallment = Application.InstallmentPayments.Commands.App.Create.CreateInstallmentPaymentCommand;
using AppCreateInstallmentHandler = Application.InstallmentPayments.Commands.App.Create.CreateInstallmentPaymentHandler;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.Orders;

public class SellerCashHolderTests
{
    private const double PrepaymentAmount = 50000;

    [Fact]
    public async Task SetSellerInfo_WhenSellerIsMandob_AddsPrepaymentToMandobNotMotaba()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db, sellerIsMotaba: false);

        var result = await SetSellerInfo(db, seed);

        Assert.True(result.IsSucceeded);
        Assert.Equal(PrepaymentAmount, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(PrepaymentAmount, Assert.Single(await db.InstallmentPayments.ToListAsync()).Amount);
        Assert.Equal(seed.InitialSafeRemaining, (await db.Safes.SingleAsync(x => x.Id == seed.SafeId)).RemainingCashAmount);
    }

    [Fact]
    public async Task SetSellerInfo_WhenSellerIsMotaba_TahirReceivesPrepaymentNotMandob()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db, sellerIsMotaba: true);

        var result = await SetSellerInfo(db, seed);

        Assert.True(result.IsSucceeded);
        var tahir = await db.Users.SingleAsync(x => x.Id == seed.MotabaId);
        Assert.Equal("طاهر عبود شنان", tahir.FullName);
        Assert.Equal(PrepaymentAmount, tahir.UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
        Assert.Equal(PrepaymentAmount, Assert.Single(await db.InstallmentPayments.ToListAsync()).Amount);
    }

    [Fact]
    public async Task Approve_WhenSellerIsMotaba_AddsPrepaymentToSellerAndRejectsSecondApproval()
    {
        await using var db = CreateDbContext();
        var seed = await SeedPendingAppOrderAsync(db, sellerIsMotaba: true);

        var handler = new ChangeOrderApprovalStatusHandler(db, new NotificationServiceStub(), new ActivityLogServiceStub());
        var command = new ChangeOrderApprovalStatusCommand
        {
            OrderId = seed.OrderId,
            ApprovalStatus = OrderApprovalStatus.Approved
        };

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.IsSucceeded);
        Assert.True(second.IsFailed);
        Assert.Equal(OrderErrors.ApprovalStatusOfOrderHasBeenSetBefore, second.Error);
        Assert.Equal(PrepaymentAmount, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
        Assert.Equal(1, await db.InstallmentPayments.CountAsync());
    }

    [Fact]
    public async Task UpdateOrder_WhenPrepaymentChanges_AdjustsSellerBalanceOnly()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db, sellerIsMotaba: true);
        Assert.True((await SetSellerInfo(db, seed)).IsSucceeded);

        var orderItem = await db.Set<OrderItem>().SingleAsync(x => x.OrderId == seed.OrderId);
        var handler = new UpdateOrderHandler(
            db,
            CurrentUser(seed.BranchId),
            new DateTimeProviderStub(),
            new NotificationServiceStub(),
            new ActivityLogServiceStub());

        var result = await handler.Handle(new UpdateOrderCommand
        {
            Id = seed.OrderId,
            CreationAddress = "النجف حي السلام",
            SaleDate = new DateOnly(2026, 8, 19),
            SaleTime = new TimeOnly(11, 10),
            SellerId = seed.SellerId,
            OrderListId = seed.OrderListId,
            OrderItems =
            [
                new OrderItemDto
                {
                    Id = orderItem.Id,
                    ProductType = ProductType.Foreign,
                    Quantity = 1,
                    BuyAmount = 0,
                    SellAmount = 1150000,
                    PrepaymentAmount = 70000,
                    DailyInstallmentAmount = 15000,
                    ProductName = "موبايل"
                }
            ]
        }, CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal(70000, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
        Assert.Equal(70000, Assert.Single(await db.InstallmentPayments.ToListAsync()).Amount);
    }

    [Fact]
    public async Task AppInstallment_WhenMotabaIsSeller_AddsToMotabaEvenIfCollectorIsMotaba()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: true);

        var handler = new AppCreateInstallmentHandler(db, new DateTimeProviderStub(), new ActivityLogServiceStub());
        var result = await handler.Handle(new AppCreateInstallment
        {
            OrderId = seed.OrderId,
            Amount = 15000,
            UserId = seed.MotabaId
        }, CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 15000, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
    }

    [Fact]
    public async Task AppSync_CreditsOrderSellerNotTheCollector()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);

        var handler = new SyncInstallmentPaymentsHandler(db, new ActivityLogServiceStub());
        var result = await handler.Handle(new SyncInstallmentPaymentsCommand
        {
            UserId = seed.MotabaId,
            InstallmentPayments =
            [
                new InstallmentPaymentDto
                {
                    OrderId = seed.OrderId,
                    Amount = 15000,
                    Date = new DateOnly(2026, 8, 19)
                }
            ]
        }, CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 15000, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
    }

    [Fact]
    public async Task AdminInstallment_AddsToSeller_AndDeleteDeductsFromSeller()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: true);
        var currentUser = CurrentUser(seed.BranchId);

        var createHandler = new AdminCreateInstallmentHandler(
            db,
            currentUser,
            new DateTimeProviderStub(),
            new ActivityLogServiceStub());
        var createResult = await createHandler.Handle(new AdminCreateInstallment
        {
            OrderId = seed.OrderId,
            Date = new DateOnly(2026, 8, 19),
            Amount = 15000,
            Description = "قسط"
        }, CancellationToken.None);

        Assert.True(createResult.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 15000, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);

        var installment = await db.InstallmentPayments.SingleAsync(x => x.Amount == 15000);
        var updateHandler = new UpdateInstallmentPaymentHandler(
            db,
            currentUser,
            new DateTimeProviderStub(),
            new ActivityLogServiceStub());
        var updateResult = await updateHandler.Handle(new UpdateInstallmentPaymentCommand
        {
            Id = installment.Id,
            Amount = 20000,
            Description = "تعديل"
        }, CancellationToken.None);

        Assert.True(updateResult.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 20000, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);

        var deleteResult = await new DeleteInstallmentPaymentHandler(db, new ActivityLogServiceStub())
            .Handle(new DeleteInstallmentPaymentCommand(installment.Id), CancellationToken.None);

        Assert.True(deleteResult.IsSucceeded);
        Assert.Equal(PrepaymentAmount, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
    }

    [Fact]
    public async Task SafeScreen_IncludesMandobAndMotaba_AndShowsEachBalance()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: true);
        (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount = 10000;
        await db.SaveChangesAsync();

        var sellers = await new GetSafeSellersHandler(db).Handle(new GetSafeSellersQuery
        {
            SafeId = seed.SafeId,
            Pagination = new Pagination(1, 10),
            Filter = new GetSafeSellersFilter()
        }, CancellationToken.None);

        Assert.True(sellers.IsSucceeded);
        Assert.Equal(2, sellers.Data!.TotalCount);
        var tahir = Assert.Single(sellers.Data.Items, x => x.Id == seed.MotabaId);
        var mandob = Assert.Single(sellers.Data.Items, x => x.Id == seed.MandobId);
        Assert.Equal(PrepaymentAmount, tahir.UndeliveredCashAmount);
        Assert.Equal("المتابع", tahir.RoleName);
        Assert.Equal(10000, mandob.UndeliveredCashAmount);
        Assert.Equal("المندوب", mandob.RoleName);

        var safe = await new GetSafeByBranchHandler(db, new CurrentUserServiceStub { Role = SeedRoles.Admin.Name })
            .Handle(new GetSafeByBranchQuery(seed.BranchId), CancellationToken.None);

        Assert.True(safe.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 10000, safe.Data!.TotalUndeliveredCashAmount);
    }

    [Fact]
    public async Task CashDelivery_WhenSellerIsMotaba_ReducesSellerAndIncreasesSafeAfterApproval()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: true);
        var remainingBefore = (await db.Safes.SingleAsync(x => x.Id == seed.SafeId)).RemainingCashAmount;

        var createResult = await new CreateCashDeliveryHandler(db, new NotificationServiceStub(), new ActivityLogServiceStub())
            .Handle(new CreateCashDeliveryCommand
            {
                SafeId = seed.SafeId,
                SellerId = seed.MotabaId,
                Amount = PrepaymentAmount,
                Date = new DateOnly(2026, 8, 19),
                Description = "تسليم"
            }, CancellationToken.None);

        Assert.True(createResult.IsSucceeded);

        var transaction = await db.Transactions.SingleAsync(x => x.Type == TransactionType.SellerPayment);
        var approveResult = await new ChangeTransactionStatusHandler(db, new ActivityLogServiceStub())
            .Handle(new ChangeTransactionStatusCommand
            {
                Id = transaction.Id,
                Status = TransactionStatus.Approved
            }, CancellationToken.None);

        Assert.True(approveResult.IsSucceeded);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(remainingBefore + PrepaymentAmount, (await db.Safes.SingleAsync(x => x.Id == seed.SafeId)).RemainingCashAmount);
    }

    [Fact]
    public async Task DailyReport_UsesOrderSellerInstallments()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: true);

        var report = await new GetUserDailyInstallmentCollectionsReportHandler(db, new DateTimeProviderStub())
            .Handle(new GetUserDailyInstallmentCollectionsReportQuery
            {
                UserId = seed.MotabaId,
                StartDate = new DateOnly(2026, 8, 19),
                EndDate = new DateOnly(2026, 8, 19)
            }, CancellationToken.None);

        Assert.True(report.IsSucceeded);
        Assert.Equal(PrepaymentAmount, report.Data!.UndeliveredCashAmount);
        Assert.Equal(PrepaymentAmount, Assert.Single(report.Data.Items).TotalInstallmentAmount);
    }

    private static async Task<Result> SetSellerInfo(TestApplicationDbContext db, Seed seed)
    {
        var handler = new SetOrderSellerInfoHandler(
            db,
            CurrentUser(seed.BranchId),
            new DateTimeProviderStub(),
            new NotificationServiceStub(),
            new ActivityLogServiceStub());

        return await handler.Handle(new SetOrderSellerInfoCommand
        {
            OrderId = seed.OrderId,
            CreationAddress = "النجف حي السلام",
            SaleDate = new DateOnly(2026, 8, 19),
            SaleTime = new TimeOnly(11, 10),
            SellerId = seed.SellerId,
            OrderListId = seed.OrderListId
        }, CancellationToken.None);
    }

    private static async Task<Seed> SeedInProgressOrderAsync(TestApplicationDbContext db, bool sellerIsMotaba)
    {
        var seed = await SeedAsync(db, sellerIsMotaba);
        var result = await SetSellerInfo(db, seed);
        Assert.True(result.IsSucceeded);
        return seed;
    }

    private static CurrentUserServiceStub CurrentUser(int branchId)
    {
        return new CurrentUserServiceStub
        {
            Role = SeedRoles.BranchAccountant.Name,
            BranchIds = [branchId]
        };
    }

    private static TestApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static async Task<Seed> SeedAsync(TestApplicationDbContext db, bool sellerIsMotaba)
    {
        var province = new Province("النجف");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var branch = new Branch("الشمالية", province.Id);
        branch.Safe!.RemainingCashAmount = 5000000;
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var mandobRole = new Role(SeedRoles.Mandob.Name, SeedRoles.Mandob.DisplayName);
        var motabaRole = new Role(SeedRoles.Motaba.Name, SeedRoles.Motaba.DisplayName);
        db.Roles.AddRange(mandobRole, motabaRole);
        await db.SaveChangesAsync();

        var mandob = CreateUser(sellerIsMotaba ? "مندوب القائمة" : "طاهر عبود شنان", "mandob");
        var motaba = CreateUser(sellerIsMotaba ? "طاهر عبود شنان" : "متابع القائمة", "motaba");
        db.Users.AddRange(mandob, motaba);
        await db.SaveChangesAsync();

        db.UserRoles.AddRange(
            new UserRole(mandob.Id, mandobRole.Id),
            new UserRole(motaba.Id, motabaRole.Id));

        var orderList = new OrderList("قائمة الشمالية", mandob.Id, motaba.Id, branch.Id);
        db.OrderLists.Add(orderList);

        var customer = new Customer(
            "زبون",
            "أم الزبون",
            "9876543210",
            new DateOnly(1985, 5, 5),
            "07711111111",
            "07711111111",
            new Business("محل", "العنوان", "علامة"),
            branch.Id);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var sellerId = sellerIsMotaba ? motaba.Id : mandob.Id;
        var order = Order.CreateByAdmin(
            0,
            1150000,
            PrepaymentAmount,
            15000,
            customer.Id,
            branch.Id,
            [
                new OrderItem(ProductType.Foreign, 1, 0, 1150000, PrepaymentAmount, 15000, "موبايل", null)
            ]);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return new Seed(
            order.Id,
            customer.Id,
            branch.Id,
            branch.Safe!.Id,
            orderList.Id,
            mandob.Id,
            motaba.Id,
            sellerId,
            branch.Safe.RemainingCashAmount);
    }

    private static async Task<Seed> SeedPendingAppOrderAsync(TestApplicationDbContext db, bool sellerIsMotaba)
    {
        var seed = await SeedAsync(db, sellerIsMotaba);
        db.Orders.Remove(await db.Orders.SingleAsync(x => x.Id == seed.OrderId));
        await db.SaveChangesAsync();

        var customerId = seed.CustomerId;
        var order = Order.CreateByAppUser(
            0,
            1150000,
            PrepaymentAmount,
            15000,
            customerId,
            seed.BranchId,
            seed.SellerId,
            [
                new OrderItem(ProductType.Foreign, 1, 0, 1150000, PrepaymentAmount, 15000, "موبايل", null)
            ]);
        order.Step = OrderStep.Completed;
        order.SaleDate = new DateOnly(2026, 8, 19);
        order.OrderListId = seed.OrderListId;
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return seed with { OrderId = order.Id };
    }

    private static User CreateUser(string fullName, string username)
    {
        return new User(
            fullName,
            "الأم",
            username + "123456",
            new DateOnly(1990, 1, 1),
            username,
            "hash",
            "العنوان",
            "07700000000");
    }

    private sealed record Seed(
        int OrderId,
        int CustomerId,
        int BranchId,
        int SafeId,
        int OrderListId,
        int MandobId,
        int MotabaId,
        int SellerId,
        double InitialSafeRemaining);
}
