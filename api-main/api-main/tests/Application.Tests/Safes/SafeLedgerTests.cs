using Application.Safes.Commands.Admin.Delete;
using Application.Safes.Common;
using Application.Safes.Queries.Admin.Verify;
using Domain.Entities.BranchAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SafeEntity = Domain.Entities.SafeAggregate.Safe;

namespace Application.Tests.Safes;

public class SafeLedgerTests
{
    [Fact]
    public async Task Withdraw_DebitsOnlyItsOwnSafe_AndTouchesNoOtherSafe()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 1_000_000, "افتتاحي", 1, CancellationToken.None);

        var result = await ledger.WithdrawAsync(
            seed.BranchSafeId, 300_000, "مصروف نقل", Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsSucceeded);

        var branchSafe = await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId);
        var expensesSafe = await db.Safes.SingleAsync(x => x.Id == seed.ExpensesSafeId);

        Assert.Equal(700_000, branchSafe.Balance);
        // سجل الصرفيات خزينة عادية: السحب لا يرفده تلقائياً
        Assert.Equal(0, expensesSafe.Balance);

        // حركة واحدة في خزينتها، بلا طرف مقابل ولا مجموعة نقل
        var movement = await db.SafeMovements
            .SingleAsync(x => x.Kind == SafeMovementKind.Withdrawal);
        Assert.Equal(seed.BranchSafeId, movement.SafeId);
        Assert.Equal(-300_000, movement.Amount);
        Assert.Null(movement.CounterpartySafeId);
        Assert.Null(movement.TransferGroupId);
    }

    [Fact]
    public async Task Withdraw_MoreThanBalance_IsRejected_AndNothingChanges()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 100_000, "افتتاحي", 1, CancellationToken.None);

        var result = await ledger.WithdrawAsync(
            seed.BranchSafeId, 150_000, "سحب زائد", Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.ThereIsNotEnoughMoneyInTheSafe, result.Error);

        var branchSafe = await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId);
        Assert.Equal(100_000, branchSafe.Balance);
        Assert.Equal(0, await db.Safes.Where(x => x.Id == seed.ExpensesSafeId).SumAsync(x => x.Balance));
    }

    [Fact]
    public async Task SameOperationKeyTwice_AppliesOnce()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);
        var key = Guid.NewGuid();

        var first = await ledger.DepositAsync(seed.BranchSafeId, 50_000, "إضافة", key, CancellationToken.None);
        var second = await ledger.DepositAsync(seed.BranchSafeId, 50_000, "إضافة", key, CancellationToken.None);

        Assert.True(first.IsSucceeded);
        Assert.True(second.IsSucceeded);

        var branchSafe = await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId);
        Assert.Equal(50_000, branchSafe.Balance);
        Assert.Equal(1, await db.SafeMovements.CountAsync(x => x.OperationKey == key));
    }

    [Fact]
    public async Task Transfer_WritesTwoBalancedRows_SharingOneTransferGroup()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 400_000, "افتتاحي", 1, CancellationToken.None);

        var result = await ledger.TransferAsync(
            seed.BranchSafeId, seed.ListSafeId, 120_000, "تمويل قائمة", Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsSucceeded);

        var rows = await db.SafeMovements
            .Where(x => x.TransferGroupId != null)
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.Single(rows.Select(x => x.TransferGroupId).Distinct());
        Assert.Equal(0, rows.Sum(x => x.Amount));
        Assert.Equal(SafeMovementKind.TransferOut, rows[0].Kind);
        Assert.Equal(SafeMovementKind.TransferIn, rows[1].Kind);
        Assert.Equal(seed.ListSafeId, rows[0].CounterpartySafeId);
        Assert.Equal(seed.BranchSafeId, rows[1].CounterpartySafeId);

        Assert.Equal(280_000, (await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId)).Balance);
        Assert.Equal(120_000, (await db.Safes.SingleAsync(x => x.Id == seed.ListSafeId)).Balance);
    }

    [Fact]
    public async Task Transfer_BetweenTwoBranches_IsRejected()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        var otherBranch = new Branch("الكرخ", seed.ProvinceId);
        db.Branches.Add(otherBranch);
        await db.SaveChangesAsync();

        var otherBranchSafeId = otherBranch.Safes.Single(x => x.Kind == SafeKind.Branch).Id;

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 200_000, "افتتاحي", 1, CancellationToken.None);

        var result = await ledger.TransferAsync(
            seed.BranchSafeId, otherBranchSafeId, 50_000, "نقل بين فرعين", Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.TransferBetweenBranchesIsNotAllowed, result.Error);
        Assert.Equal(200_000, (await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId)).Balance);
    }

    [Fact]
    public async Task Verify_ReportsBalanced_WhenLedgerMatchesStoredBalance()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 500_000, "افتتاحي", 1, CancellationToken.None);
        await ledger.DepositAsync(seed.BranchSafeId, 25_000, "إضافة", Guid.NewGuid(), CancellationToken.None);
        await ledger.WithdrawAsync(seed.BranchSafeId, 75_000, "سحب", Guid.NewGuid(), CancellationToken.None);

        var verify = await new VerifySafeBalanceHandler(db, new CurrentUserServiceStub { BranchIds = [seed.BranchId] })
            .Handle(new VerifySafeBalanceQuery(seed.BranchSafeId), CancellationToken.None);

        Assert.True(verify.IsSucceeded);
        Assert.True(verify.Data!.IsBalanced);
        Assert.Equal(450_000, verify.Data.StoredBalance);
        Assert.Equal(450_000, verify.Data.LedgerBalance);
        Assert.Equal(0, verify.Data.Difference);
        Assert.Equal(0, verify.Data.InconsistentSnapshots);
        Assert.Equal(3, verify.Data.MovementsCount);
    }

    [Fact]
    public async Task Verify_DetectsTamperedBalance()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.BranchSafeId, 100_000, "افتتاحي", 1, CancellationToken.None);

        var safe = await db.Safes.SingleAsync(x => x.Id == seed.BranchSafeId);
        safe.TryApplyDelta(40_000);
        await db.SaveChangesAsync();

        var verify = await new VerifySafeBalanceHandler(db, new CurrentUserServiceStub { BranchIds = [seed.BranchId] })
            .Handle(new VerifySafeBalanceQuery(seed.BranchSafeId), CancellationToken.None);

        Assert.False(verify.Data!.IsBalanced);
        Assert.Equal(40_000, verify.Data.Difference);
    }

    [Fact]
    public async Task Delete_SystemSafe_IsRejected()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);

        var result = await DeleteHandler(db, seed.BranchId)
            .Handle(new DeleteSafeCommand(seed.BranchSafeId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.SystemSafeCannotBeDeleted, result.Error);
    }

    [Fact]
    public async Task Delete_SafeWithBalance_IsRejected()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.ListSafeId, 10_000, "افتتاحي", 1, CancellationToken.None);

        var result = await DeleteHandler(db, seed.BranchId)
            .Handle(new DeleteSafeCommand(seed.ListSafeId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.SafeWithBalanceCannotBeDeleted, result.Error);
    }

    [Fact]
    public async Task Delete_EmptySafeWithMovementHistory_IsRejected()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        await ledger.ApplyOpeningAsync(seed.ListSafeId, 10_000, "افتتاحي", 1, CancellationToken.None);
        await ledger.WithdrawAsync(seed.ListSafeId, 10_000, "تصفير", Guid.NewGuid(), CancellationToken.None);

        Assert.True((await db.Safes.SingleAsync(x => x.Id == seed.ListSafeId)).IsEmpty);

        var result = await DeleteHandler(db, seed.BranchId)
            .Handle(new DeleteSafeCommand(seed.ListSafeId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(SafeErrors.SafeWithMovementsCannotBeDeleted, result.Error);
    }

    [Fact]
    public async Task Opening_CarriesExactUndeliveredAmount_AndIsAuditable()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);
        var ledger = Ledger(db);

        const double undelivered = 1_234_000;

        var result = await ledger.ApplyOpeningAsync(
            seed.ListSafeId, undelivered, "رصيد افتتاحي مُرحَّل من UndeliveredCashAmount", 7, CancellationToken.None);

        Assert.True(result.IsSucceeded);

        var movement = await db.SafeMovements.SingleAsync(x => x.SafeId == seed.ListSafeId);
        Assert.Equal(SafeMovementKind.Opening, movement.Kind);
        Assert.Equal(undelivered, movement.Amount);
        Assert.Equal(undelivered, movement.BalanceAfter);
        Assert.Equal(7, movement.PerformedByUserId);
        Assert.Equal(undelivered, (await db.Safes.SingleAsync(x => x.Id == seed.ListSafeId)).Balance);
    }

    [Fact]
    public async Task BranchConstructor_CreatesBranchAndExpensesSystemSafes()
    {
        await using var db = CreateDb();
        var seed = await SeedAsync(db);

        var safes = await db.Safes.Where(x => x.BranchId == seed.BranchId).ToListAsync();

        var branchSafe = safes.Single(x => x.Kind == SafeKind.Branch);
        var expensesSafe = safes.Single(x => x.Kind == SafeKind.Expenses);

        Assert.True(branchSafe.IsSystem);
        Assert.True(expensesSafe.IsSystem);
        Assert.False(branchSafe.CanBeDeleted);
        Assert.Equal("قاصة الرصافة", branchSafe.Name);
        Assert.StartsWith("سجل الصرفيات", expensesSafe.Name);
    }

    private static ISafeLedger Ledger(TestApplicationDbContext db) =>
        new SafeLedger(db, new CurrentUserServiceStub());

    private static DeleteSafeHandler DeleteHandler(TestApplicationDbContext db, int branchId) =>
        new(db, new CurrentUserServiceStub { BranchIds = [branchId] }, new ActivityLogServiceStub());

    private static TestApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestApplicationDbContext(options);
    }

    private static async Task<Seed> SeedAsync(TestApplicationDbContext db)
    {
        var province = new Province("بغداد");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var branch = new Branch("الرصافة", province.Id);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var listSafe = SafeEntity.ForOrderList("قائمة أ", branch.Id, 0);
        db.Safes.Add(listSafe);
        await db.SaveChangesAsync();

        return new Seed(
            province.Id,
            branch.Id,
            branch.Safes.Single(x => x.Kind == SafeKind.Branch).Id,
            branch.Safes.Single(x => x.Kind == SafeKind.Expenses).Id,
            listSafe.Id);
    }

    private record Seed(int ProvinceId, int BranchId, int BranchSafeId, int ExpensesSafeId, int ListSafeId);
}
