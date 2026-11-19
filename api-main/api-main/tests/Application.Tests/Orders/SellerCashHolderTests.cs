using Application.Safes.Common;
using Application.Common.Models;
using Application.InstallmentPayments.Commands.Admin.Create;
using Application.InstallmentPayments.Commands.Admin.Delete;
using Application.InstallmentPayments.Commands.Admin.Update;
using Application.InstallmentPayments.Commands.App.Create;
using Application.InstallmentPayments.Commands.App.Sync;
using Application.InstallmentPayments.Common;
using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Commands.Admin.SetSellerInfo;
using Application.Orders.Commands.Admin.Update;
using Application.Orders.Common;
using Application.Tests;
using Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
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
        Assert.Equal(seed.InitialSafeRemaining, (await db.Safes.SingleAsync(x => x.Id == seed.SafeId)).Balance);
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

        var handler = new ChangeOrderApprovalStatusHandler(db, new NotificationServiceStub(), new ActivityLogServiceStub(),
            new SafeLedger(db, new CurrentUserServiceStub()));
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
            new ActivityLogServiceStub(),
            new SafeLedger(db, new CurrentUserServiceStub()));

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

        var writer = Writer(db);
        var handler = new AppCreateInstallmentHandler(writer);
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

        var writer = Writer(db);
        var handler = new SyncInstallmentPaymentsHandler(writer);
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
            new ActivityLogServiceStub(),
            Posting(db, currentUser));
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
            new ActivityLogServiceStub(),
            Posting(db, currentUser));
        var updateResult = await updateHandler.Handle(new UpdateInstallmentPaymentCommand
        {
            Id = installment.Id,
            Amount = 20000,
            Description = "تعديل"
        }, CancellationToken.None);

        Assert.True(updateResult.IsSucceeded);
        Assert.Equal(PrepaymentAmount + 20000, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);

        var deleteResult = await new DeleteInstallmentPaymentHandler(db, new ActivityLogServiceStub(), Posting(db, currentUser), currentUser)
            .Handle(new DeleteInstallmentPaymentCommand(installment.Id), CancellationToken.None);

        Assert.True(deleteResult.IsSucceeded);
        Assert.Equal(PrepaymentAmount, (await db.Users.SingleAsync(x => x.Id == seed.MotabaId)).UndeliveredCashAmount);
        Assert.Equal(0, (await db.Users.SingleAsync(x => x.Id == seed.MandobId)).UndeliveredCashAmount);
    }



    [Fact]
    public async Task AppInstallment_GoesToListSafe_EvenWhenMotabaHasOwnSafe()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        db.Safes.Add(Domain.Entities.SafeAggregate.Safe.ForEmployee("المتابع", seed.BranchId, seed.MotabaId));
        await db.SaveChangesAsync();

        var result = await new AppCreateInstallmentHandler(Writer(db)).Handle(new AppCreateInstallment
        {
            OrderId = seed.OrderId,
            Amount = 15000,
            UserId = seed.MotabaId
        }, CancellationToken.None);

        Assert.True(result.IsSucceeded, result.Error?.Message);
        var listSafe = await db.Safes.SingleAsync(x => x.OrderListId == seed.OrderListId);
        Assert.Equal(15000, listSafe.Balance);
        Assert.Equal(0, (await db.Safes.SingleAsync(x => x.UserId == seed.MotabaId)).Balance);
        var movement = await db.SafeMovements.SingleAsync(x => x.SafeId == listSafe.Id);
        Assert.Equal(SafeMovementKind.Collection, movement.Kind);
        Assert.Equal(seed.MotabaId, movement.PerformedByUserId);
        Assert.Equal((await db.InstallmentPayments.SingleAsync(x => x.Amount == 15000)).Id, movement.InstallmentPaymentId);
    }

    [Fact]
    public async Task AppSync_RetryWithSameKey_PostsOnce()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        var key = Guid.NewGuid();
        var command = new SyncInstallmentPaymentsCommand
        {
            UserId = seed.MotabaId,
            InstallmentPayments =
            [
                new InstallmentPaymentDto
                {
                    OrderId = seed.OrderId,
                    Amount = 15000,
                    Date = new DateOnly(2026, 8, 19),
                    ClientPaymentId = key,
                    CheckLegacyPayments = false
                }
            ]
        };

        Assert.True((await new SyncInstallmentPaymentsHandler(Writer(db)).Handle(command, CancellationToken.None)).IsSucceeded);
        Assert.True((await new SyncInstallmentPaymentsHandler(Writer(db)).Handle(command, CancellationToken.None)).IsSucceeded);

        var listSafe = await db.Safes.SingleAsync(x => x.OrderListId == seed.OrderListId);
        Assert.Equal(15000, listSafe.Balance);
        Assert.Single(await db.SafeMovements.Where(x => x.SafeId == listSafe.Id).ToListAsync());
    }

    [Fact]
    public async Task AdminInstallment_PostsToListSafe_UpdateAdjusts_DeleteReverses()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        var currentUser = CurrentUser(seed.BranchId);

        var created = await new AdminCreateInstallmentHandler(db, currentUser, new DateTimeProviderStub(), new ActivityLogServiceStub(), Posting(db, currentUser))
            .Handle(new AdminCreateInstallment { OrderId = seed.OrderId, Date = new DateOnly(2026, 8, 19), Amount = 15000 }, CancellationToken.None);
        Assert.True(created.IsSucceeded, created.Error?.Message);
        Assert.Equal(15000, await ListBalance(db, seed));

        var installment = await db.InstallmentPayments.SingleAsync(x => x.Amount == 15000);
        var updated = await new UpdateInstallmentPaymentHandler(db, currentUser, new DateTimeProviderStub(), new ActivityLogServiceStub(), Posting(db, currentUser))
            .Handle(new UpdateInstallmentPaymentCommand { Id = installment.Id, Amount = 10000 }, CancellationToken.None);
        Assert.True(updated.IsSucceeded, updated.Error?.Message);
        Assert.Equal(10000, await ListBalance(db, seed));

        var deleted = await new DeleteInstallmentPaymentHandler(db, new ActivityLogServiceStub(), Posting(db, currentUser), currentUser)
            .Handle(new DeleteInstallmentPaymentCommand(installment.Id), CancellationToken.None);
        Assert.True(deleted.IsSucceeded, deleted.Error?.Message);
        Assert.Equal(0, await ListBalance(db, seed));
        Assert.Equal(0, (await db.SafeMovements.Where(x => x.InstallmentPaymentId == installment.Id).ToListAsync()).Sum(x => x.Amount));
    }

    [Fact]
    public async Task Delete_IsRefused_WhenListSafeNoLongerHoldsTheMoney()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        var currentUser = CurrentUser(seed.BranchId);
        var paid = await new AppCreateInstallmentHandler(Writer(db)).Handle(
            new AppCreateInstallment { OrderId = seed.OrderId, Amount = 15000, UserId = seed.MotabaId }, CancellationToken.None);
        Assert.True(paid.IsSucceeded, paid.Error?.Message);
        var listSafe = await db.Safes.SingleAsync(x => x.OrderListId == seed.OrderListId);
        Assert.True((await new SafeLedger(db, currentUser).WithdrawAsync(listSafe.Id, 15000, "تسليم", null, CancellationToken.None)).IsSucceeded);

        var installment = await db.InstallmentPayments.SingleAsync(x => x.Amount == 15000);
        var result = await new DeleteInstallmentPaymentHandler(db, new ActivityLogServiceStub(), Posting(db, currentUser), currentUser)
            .Handle(new DeleteInstallmentPaymentCommand(installment.Id), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.ListSafeCannotCoverPaymentReversal.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task AppInstallment_IsRefused_WhenListSafeIsInactive()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        var listSafe = Domain.Entities.SafeAggregate.Safe.ForOrderList("قائمة", seed.BranchId, seed.OrderListId);
        listSafe.Deactivate();
        db.Safes.Add(listSafe);
        await db.SaveChangesAsync();

        var result = await new AppCreateInstallmentHandler(Writer(db)).Handle(new AppCreateInstallment
        {
            OrderId = seed.OrderId,
            Amount = 15000,
            UserId = seed.MotabaId
        }, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.SafeIsInactive.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Backfill_PostsOnlyEnteredPayments_AndIsIdempotent()
    {
        await using var db = CreateDbContext();
        var seed = await SeedInProgressOrderAsync(db, sellerIsMotaba: false);
        var order = await db.Orders.Include(x => x.InstallmentPayments).SingleAsync(x => x.Id == seed.OrderId);
        var entered = new InstallmentPayment(15000, new DateOnly(2026, 8, 19));
        var imported = new InstallmentPayment(9000, new DateOnly(2026, 1, 1)) { ImportKey = "najaf|CustomersPayments|1" };
        order.InstallmentPayments.Add(entered);
        order.InstallmentPayments.Add(imported);
        await db.SaveChangesAsync();
        db.ActivityLogs.Add(new Domain.Entities.ActivityLogAggregate.ActivityLog(
            Domain.Entities.ActivityLogAggregate.Enums.ActivityType.InstallmentPaymentsSynced, "", "", "",
            Domain.Entities.ActivityLogAggregate.Enums.TargetEntityType.InstallmentPayment, entered.Id,
            null, null, default, null, null, seed.BranchId, seed.MotabaId));
        await db.SaveChangesAsync();

        var currentUser = CurrentUser(seed.BranchId);
        var handler = new Application.Safes.Commands.Admin.BackfillPaymentCollections.BackfillPaymentCollectionsHandler(
            db, currentUser, new SafeLedger(db, currentUser), new ActivityLogServiceStub());

        var preview = await handler.Handle(new() { BranchId = seed.BranchId, DryRun = true }, CancellationToken.None);
        Assert.True(preview.IsSucceeded, preview.Error?.Message);
        Assert.Equal(1, preview.Data!.Count);
        Assert.Equal(15000, preview.Data.Amount);
        Assert.Equal(1, preview.Data.SkippedImported);
        Assert.Equal(1, preview.Data.SkippedNotEnteredAsPayment);
        Assert.Equal(0, await db.SafeMovements.CountAsync(x => x.InstallmentPaymentId != null));

        var run = await handler.Handle(new() { BranchId = seed.BranchId, DryRun = false }, CancellationToken.None);
        var again = await handler.Handle(new() { BranchId = seed.BranchId, DryRun = false }, CancellationToken.None);

        Assert.True(run.IsSucceeded, run.Error?.Message);
        Assert.Equal(1, run.Data!.Count);
        Assert.Equal(0, again.Data!.Count);
        Assert.Equal(1, again.Data.AlreadyPosted);
        Assert.Equal(15000, await ListBalance(db, seed));
    }

    private static async Task<double> ListBalance(TestApplicationDbContext db, Seed seed)
        => (await db.Safes.SingleAsync(x => x.OrderListId == seed.OrderListId)).Balance;

    private static AppInstallmentPaymentWriter Writer(TestApplicationDbContext db)
        => new(db, new DateTimeProviderStub(), new ActivityLogServiceStub(), Posting(db, new CurrentUserServiceStub()));

    private static PaymentSafePosting Posting(TestApplicationDbContext db, CurrentUserServiceStub currentUser)
        => new(db, new SafeLedger(db, currentUser));

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
            new ActivityLogServiceStub(),
            new SafeLedger(db, new CurrentUserServiceStub()));

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
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static async Task<Seed> SeedAsync(TestApplicationDbContext db, bool sellerIsMotaba)
    {
        var province = new Province("النجف");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var branch = new Branch("الشمالية", province.Id);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var branchSafe = branch.Safes.Single(x => x.Kind == SafeKind.Branch);
        branchSafe.TryApplyDelta(5000000);
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
            branchSafe.Id,
            orderList.Id,
            mandob.Id,
            motaba.Id,
            sellerId,
            branchSafe.Balance);
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
