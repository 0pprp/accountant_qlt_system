using Application.Common.Interfaces;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SafeEntity = Domain.Entities.SafeAggregate.Safe;

namespace Application.Safes.Common;

public class SafeLedger : ISafeLedger
{
    /// <summary>namespace قفل الخزينة — منفصل عن 791901/791902/791903 المستخدمة في مسار التسديدات.</summary>
    public const int SafeLockNamespace = 791905;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public SafeLedger(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public Task<Result<SafeMovement>> DepositAsync(
        int safeId, double amount, string? note, Guid? operationKey, CancellationToken cancellationToken)
    {
        var actor = Actor(null);
        return ExecuteAsync(
            operationKey,
            async () =>
            {
                var prepared = await PrepareAsync(safeId, Math.Abs(amount), cancellationToken);
                if (prepared.IsFailed)
                    return prepared.Error;

                var (_, balanceAfter) = prepared.Data!;
                return Stage(SafeMovement.Deposit(safeId, amount, balanceAfter, note, actor, operationKey));
            },
            cancellationToken);
    }

    public Task<Result<SafeMovement>> WithdrawAsync(
        int safeId, double amount, string? note, Guid? operationKey, CancellationToken cancellationToken)
    {
        var actor = Actor(null);
        return ExecuteAsync(
            operationKey,
            async () =>
            {
                var magnitude = Math.Abs(amount);
                var prepared = await PrepareAsync(safeId, -magnitude, cancellationToken);
                if (prepared.IsFailed)
                    return prepared.Error;

                var (_, balanceAfter) = prepared.Data!;
                return Stage(SafeMovement.Withdrawal(safeId, magnitude, balanceAfter, note, actor, operationKey));
            },
            cancellationToken);
    }

    public Task<Result<SafeMovement>> TransferAsync(
        int sourceSafeId, int destinationSafeId, double amount, string? note, Guid? operationKey,
        CancellationToken cancellationToken)
    {
        var actor = Actor(null);
        return ExecuteAsync(
            operationKey,
            async () =>
            {
                if (sourceSafeId == destinationSafeId)
                    return SafeErrors.TransferToSameSafeIsNotAllowed;

                var pair = await _dbContext.Safes.AsNoTracking()
                    .Where(x => x.Id == sourceSafeId || x.Id == destinationSafeId)
                    .Select(x => new { x.Id, x.BranchId })
                    .ToListAsync(cancellationToken);

                if (pair.Count != 2)
                    return SafeErrors.SafeNotFound;

                if (pair[0].BranchId != pair[1].BranchId)
                    return SafeErrors.TransferBetweenBranchesIsNotAllowed;

                var group = Guid.NewGuid();
                var magnitude = Math.Abs(amount);

                return await MovePairAsync(
                    sourceSafeId,
                    destinationSafeId,
                    magnitude,
                    (balanceAfter, counterpartyId) => SafeMovement.TransferOut(
                        sourceSafeId, magnitude, balanceAfter, note, actor, operationKey, counterpartyId, group),
                    (balanceAfter, sourceId) => SafeMovement.TransferIn(
                        destinationSafeId, magnitude, balanceAfter, note, actor, sourceId, group),
                    cancellationToken);
            },
            cancellationToken);
    }

    public Task<Result<SafeMovement>> RecordAsync(SafeLedgerEntry entry, CancellationToken cancellationToken)
    {
        var actor = Actor(entry.PerformedByUserId);
        var magnitude = Math.Abs(entry.Amount);

        return ExecuteAsync(
            null,
            async () =>
            {
                var signed = IsCredit(entry.Kind) ? magnitude : -magnitude;
                var prepared = await PrepareAsync(entry.SafeId, signed, cancellationToken);
                if (prepared.IsFailed)
                    return prepared.Error;

                var (_, balanceAfter) = prepared.Data!;

                SafeMovement movement = entry.Kind switch
                {
                    SafeMovementKind.Collection => SafeMovement.Collection(
                        entry.SafeId, magnitude, balanceAfter, actor,
                        entry.InstallmentPaymentId ?? 0, entry.OrderId ?? 0, entry.Note),
                    SafeMovementKind.Withdrawal when entry.InstallmentPaymentId is int reversedPaymentId => SafeMovement.CollectionReversal(
                        entry.SafeId, magnitude, balanceAfter, entry.Note, actor, reversedPaymentId, entry.OrderId),
                    SafeMovementKind.Prepayment => SafeMovement.Prepayment(
                        entry.SafeId, magnitude, balanceAfter, actor, entry.OrderId ?? 0),
                    SafeMovementKind.ExpenseOut => SafeMovement.ExpenseOut(
                        entry.SafeId, magnitude, balanceAfter, entry.Note, actor, entry.ExpenseId ?? 0),
                    SafeMovementKind.PurchaseOut => SafeMovement.PurchaseOut(
                        entry.SafeId, magnitude, balanceAfter, entry.Note, actor, entry.PurchaseId, entry.OrderId),
                    SafeMovementKind.Refund => SafeMovement.Refund(
                        entry.SafeId, magnitude, balanceAfter, entry.Note, actor,
                        entry.ExpenseId, entry.PurchaseId, entry.OrderId),
                    _ => SafeMovement.Deposit(entry.SafeId, magnitude, balanceAfter, entry.Note, actor, null)
                };

                return Stage(movement);
            },
            cancellationToken);
    }

    public Task<Result<SafeMovement>> ApplyOpeningAsync(
        int safeId, double signedAmount, string? note, int performedByUserId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            null,
            async () =>
            {
                var prepared = await PrepareAsync(safeId, signedAmount, cancellationToken);
                if (prepared.IsFailed)
                    return prepared.Error;

                var (_, balanceAfter) = prepared.Data!;
                return Stage(SafeMovement.Opening(safeId, signedAmount, balanceAfter, note, performedByUserId));
            },
            cancellationToken);

    public async Task<Result<SafeEntity>> GetBranchSafeAsync(int branchId, CancellationToken cancellationToken)
    {
        var safe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Kind == SafeKind.Branch, cancellationToken);

        if (safe is null)
            return SafeErrors.BranchSafeNotFound;

        return safe;
    }

    public async Task<Result<SafeEntity>> GetExpensesSafeAsync(int branchId, CancellationToken cancellationToken)
    {
        var safe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Kind == SafeKind.Expenses, cancellationToken);

        if (safe is null)
            return SafeErrors.ExpensesSafeNotFound;

        return safe;
    }

    public async Task<Result<SafeEntity>> ResolveCollectionSafeAsync(
        int orderListId, int collectorUserId, CancellationToken cancellationToken)
    {
        var list = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.Id == orderListId)
            .Select(x => new { x.Id, x.BranchId, x.MotabaId })
            .FirstOrDefaultAsync(cancellationToken);

        if (list is null)
            return SafeErrors.OrderListNotFoundInBranch;

        if (list.MotabaId == collectorUserId)
        {
            var employeeSafe = await _dbContext.Safes
                .FirstOrDefaultAsync(
                    x => x.BranchId == list.BranchId && x.UserId == collectorUserId, cancellationToken);

            if (employeeSafe is not null)
                return employeeSafe;
        }

        var listSafe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.OrderListId == orderListId, cancellationToken);

        if (listSafe is null)
            return SafeErrors.SafeNotFound;

        return listSafe;
    }

    private static bool IsCredit(SafeMovementKind kind) =>
        kind is SafeMovementKind.Opening
            or SafeMovementKind.Deposit
            or SafeMovementKind.TransferIn
            or SafeMovementKind.Collection
            or SafeMovementKind.Prepayment
            or SafeMovementKind.Refund;

    private int Actor(int? explicitUserId) => explicitUserId ?? _currentUser.UserId ?? 0;

    private SafeMovement Stage(SafeMovement movement)
    {
        _dbContext.SafeMovements.Add(movement);
        return movement;
    }

    /// <summary>يقفل الخزينة ثم يطبّق التغيير على الرصيد، ويُرجع الرصيد الجديد.</summary>
    private async Task<Result<(SafeEntity Safe, double BalanceAfter)>> PrepareAsync(
        int safeId, double signedAmount, CancellationToken cancellationToken)
    {
        await AcquireSafeLockAsync(safeId, cancellationToken);

        var safe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == safeId, cancellationToken);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        if (safe.IsActive == false)
            return SafeErrors.SafeIsInactive;

        var balanceAfter = safe.TryApplyDelta(signedAmount);
        if (balanceAfter is null)
            return SafeErrors.ThereIsNotEnoughMoneyInTheSafe;

        return (safe, balanceAfter.Value);
    }

    /// <summary>
    /// حركة خروج ودخول متقابلتان. القفل بترتيب تصاعدي للمعرّفين إلزامي لتجنّب deadlock
    /// عند نقلين متعاكسين في نفس اللحظة.
    /// </summary>
    private async Task<Result<SafeMovement>> MovePairAsync(
        int sourceSafeId,
        int destinationSafeId,
        double magnitude,
        Func<double, int, SafeMovement> buildOut,
        Func<double, int, SafeMovement> buildIn,
        CancellationToken cancellationToken)
    {
        var first = Math.Min(sourceSafeId, destinationSafeId);
        var second = Math.Max(sourceSafeId, destinationSafeId);
        await AcquireSafeLockAsync(first, cancellationToken);
        await AcquireSafeLockAsync(second, cancellationToken);

        var source = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == sourceSafeId, cancellationToken);
        var destination = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.Id == destinationSafeId, cancellationToken);

        if (source is null || destination is null)
            return SafeErrors.SafeNotFound;

        if (source.IsActive == false || destination.IsActive == false)
            return SafeErrors.SafeIsInactive;

        var sourceBalance = source.TryApplyDelta(-magnitude);
        if (sourceBalance is null)
            return SafeErrors.ThereIsNotEnoughMoneyInTheSafe;

        var destinationBalance = destination.TryApplyDelta(magnitude);
        if (destinationBalance is null)
            return SafeErrors.ThereIsNotEnoughMoneyInTheSafe;

        var outgoing = Stage(buildOut(sourceBalance.Value, destinationSafeId));
        Stage(buildIn(destinationBalance.Value, sourceSafeId));

        return outgoing;
    }

    /// <summary>
    /// ينفّذ العملية داخل معاملة. إن كانت هناك معاملة قائمة من الـhandler فينضم إليها ولا يعتمدها،
    /// فتبقى الذرية بيد المُنادي.
    /// </summary>
    private async Task<Result<SafeMovement>> ExecuteAsync(
        Guid? operationKey,
        Func<Task<Result<SafeMovement>>> work,
        CancellationToken cancellationToken)
    {
        var ownsTransaction = _dbContext.Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            if (operationKey is Guid key)
            {
                var replay = await _dbContext.SafeMovements.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.OperationKey == key, cancellationToken);

                if (replay is not null)
                {
                    if (ownsTransaction)
                        await transaction!.CommitAsync(cancellationToken);

                    return replay;
                }
            }

            var result = await work();
            if (result.IsFailed)
            {
                if (ownsTransaction)
                    await transaction!.RollbackAsync(cancellationToken);

                return result.Error;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (ownsTransaction)
                await transaction!.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            if (ownsTransaction)
                await transaction!.RollbackAsync(cancellationToken);

            throw;
        }
        finally
        {
            if (ownsTransaction)
                await transaction!.DisposeAsync();
        }
    }

    private async Task AcquireSafeLockAsync(int safeId, CancellationToken cancellationToken)
    {
        if (IsNpgsql() == false)
            return;

        await _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0}, {1})",
            [SafeLockNamespace, safeId],
            cancellationToken);
    }

    private bool IsNpgsql()
    {
        var provider = _dbContext.Database.ProviderName ?? "";
        return provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    }
}
