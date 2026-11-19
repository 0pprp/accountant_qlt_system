using Application.Common.Interfaces;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SafeEntity = Domain.Entities.SafeAggregate.Safe;

namespace Application.Safes.Common;

/// <summary>
/// يربط التسديد بخزينة قائمته: كل تسديد يدخل قاصة القائمة كحركة «تحصيل»، والحذف أو تغيير المبلغ
/// يكتب حركة معاكسة مربوطة بنفس التسديد، فيبقى مجموع حركات التسديد مساوياً لمبلغه.
/// </summary>
public class PaymentSafePosting
{
    public const string PrepaymentNote = "مقدمة";

    private readonly IApplicationDbContext _dbContext;
    private readonly ISafeLedger _ledger;

    public PaymentSafePosting(IApplicationDbContext dbContext, ISafeLedger ledger)
    {
        _dbContext = dbContext;
        _ledger = ledger;
    }

    /// <summary>يُدخل التسديد قاصة قائمته. لا يكرر الحركة إن كان التسديد مُرحَّلاً.</summary>
    public async Task<Result> PostAsync(
        int paymentId, int orderId, int orderListId, double amount, int performedByUserId,
        CancellationToken cancellationToken, string? note = null)
    {
        if (amount <= 0)
            return Result.Success();

        var posted = await _dbContext.SafeMovements.AsNoTracking()
            .AnyAsync(x => x.InstallmentPaymentId == paymentId, cancellationToken);
        if (posted)
            return Result.Success();

        var safe = await GetOrCreateListSafeAsync(orderListId, cancellationToken);
        if (safe.IsFailed)
            return safe.Error!;

        var recorded = await _ledger.RecordAsync(new SafeLedgerEntry
        {
            SafeId = safe.Data!.Id,
            Kind = SafeMovementKind.Collection,
            Amount = amount,
            Note = note,
            PerformedByUserId = performedByUserId,
            InstallmentPaymentId = paymentId,
            OrderId = orderId
        }, cancellationToken);

        return recorded.IsFailed ? recorded.Error! : Result.Success();
    }

    /// <summary>
    /// يضبط ما في القاصة ليساوي المبلغ الجديد. التسديد غير المُرحَّل أصلاً لا يُلمس.
    /// المبلغ صفر يعني حذف التسديد.
    /// </summary>
    public async Task<Result> AdjustAsync(
        int paymentId, double newAmount, int? performedByUserId, CancellationToken cancellationToken)
    {
        var movements = await _dbContext.SafeMovements.AsNoTracking()
            .Where(x => x.InstallmentPaymentId == paymentId)
            .OrderBy(x => x.Id)
            .Select(x => new { x.SafeId, x.Amount, x.OrderId })
            .ToListAsync(cancellationToken);

        if (movements.Count == 0)
            return Result.Success();

        var delta = Math.Round(newAmount - movements.Sum(x => x.Amount), 2);
        if (Math.Abs(delta) < 0.005)
            return Result.Success();

        var first = movements[0];
        var recorded = await _ledger.RecordAsync(new SafeLedgerEntry
        {
            SafeId = first.SafeId,
            Kind = delta > 0 ? SafeMovementKind.Collection : SafeMovementKind.Withdrawal,
            Amount = Math.Abs(delta),
            Note = delta > 0 ? null : newAmount <= 0 ? "عكس تسديد محذوف" : "عكس فرق تعديل تسديد",
            PerformedByUserId = performedByUserId,
            InstallmentPaymentId = paymentId,
            OrderId = first.OrderId
        }, cancellationToken);

        if (recorded.IsFailed)
            return recorded.Error == SafeErrors.ThereIsNotEnoughMoneyInTheSafe
                ? SafeErrors.ListSafeCannotCoverPaymentReversal
                : recorded.Error!;

        return Result.Success();
    }

    public async Task<Result> ReverseManyAsync(
        IEnumerable<int> paymentIds, int? performedByUserId, CancellationToken cancellationToken)
    {
        foreach (var id in paymentIds.Distinct())
        {
            var reversed = await AdjustAsync(id, 0, performedByUserId, cancellationToken);
            if (reversed.IsFailed)
                return reversed;
        }

        return Result.Success();
    }

    private async Task<Result<SafeEntity>> GetOrCreateListSafeAsync(int orderListId, CancellationToken cancellationToken)
    {
        var safe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.OrderListId == orderListId, cancellationToken);
        if (safe is not null)
            return safe;

        var list = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.Id == orderListId)
            .Select(x => new { x.Name, x.BranchId })
            .FirstOrDefaultAsync(cancellationToken);
        if (list is null)
            return SafeErrors.OrderListNotFoundInBranch;

        safe = SafeEntity.ForOrderList(list.Name, list.BranchId, orderListId);
        _dbContext.Safes.Add(safe);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return safe;
    }
}
