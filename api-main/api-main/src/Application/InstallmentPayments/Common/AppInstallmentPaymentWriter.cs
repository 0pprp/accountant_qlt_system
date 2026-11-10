using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.Orders.Common;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Common;

public record InstallmentPaymentWriteResultDto(
    int PaymentId,
    int OrderId,
    double Amount,
    DateOnly Date,
    Guid? ClientPaymentId,
    bool AlreadyApplied);

/// <summary>
/// مسار كتابة موحّد لـ Create و Sync — ClientPaymentId مربوط بالمالك والطلب والمبلغ والتاريخ.
/// </summary>
public class AppInstallmentPaymentWriter
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activity;

    public AppInstallmentPaymentWriter(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        IActivityLogService activity)
    {
        _db = db;
        _clock = clock;
        _activity = activity;
    }

    public async Task<Result<InstallmentPaymentWriteResultDto>> WriteAsync(
        int userId,
        int orderId,
        double amount,
        DateOnly? collectionDate,
        Guid? clientPaymentId,
        ActivityType activityType,
        CancellationToken ct)
    {
        if (amount <= 0)
            return OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount;

        var date = collectionDate ?? DateOnly.FromDateTime(_clock.UtcNow.AddHours(3).Date);
        var key = clientPaymentId is Guid g && g != Guid.Empty ? g : (Guid?)null;

        // قفل ترتيب العملية: الطلب ثم حامل النقد داخل نفس المعاملة.
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await AcquireOrderLockAsync(orderId, ct);

        if (key is Guid idemKey)
        {
            var existing = await _db.InstallmentPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClientPaymentId == idemKey, ct);

            if (existing is not null)
            {
                var decision = EvaluateExisting(existing, userId, orderId, amount, date);
                if (decision.IsFailed)
                {
                    await tx.RollbackAsync(ct);
                    return decision.Error!;
                }

                await tx.CommitAsync(ct);
                return decision;
            }
        }

        var order = await _db.Orders
            .Include(x => x.InstallmentPayments)
            .Include(x => x.OrderList)
            .FirstOrDefaultAsync(
                x => x.Id == orderId
                     && (x.OrderList!.MandobId == userId || x.OrderList.MotabaId == userId),
                ct);

        if (order is null)
        {
            await tx.RollbackAsync(ct);
            return OrderErrors.OrderNotFound;
        }

        // دفعة جديدة غير مسموحة على بيع مكتمل — إعادة الإقرار تُعالَج أعلاه فقط.
        if (order.ExecutionStatus != OrderExecutionStatus.InProgress)
        {
            await tx.RollbackAsync(ct);
            return OrderErrors.OrderNotFound;
        }

        var list = order.OrderList;
        if (list is null)
        {
            await tx.RollbackAsync(ct);
            return OrderErrors.OrderNotFound;
        }

        var collectionEnabled = await _db.OrderLists
            .Where(l => l.Id == list.Id)
            .Select(l => l.FollowerCollectionEnabled)
            .FirstAsync(ct);
        if (collectionEnabled && list.MandobId == userId)
        {
            await tx.RollbackAsync(ct);
            return FollowerWorkErrors.MandobCollectionLocked;
        }

        if (order.SellerId is int sellerId)
            await AcquireCashHolderLockAsync(sellerId, ct);

        var totalPaid = order.InstallmentPayments.Sum(x => x.Amount);
        if (totalPaid + amount > order.SellAmount)
        {
            await tx.RollbackAsync(ct);
            return OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount;
        }

        var cashHolderResult = await OrderCashHolder.GetAsync(_db, order.SellerId, ct);
        if (cashHolderResult.IsFailed)
        {
            await tx.RollbackAsync(ct);
            return cashHolderResult.Error!;
        }

        var payment = new InstallmentPayment(amount, date, clientPaymentId: key);
        // ربط صريح بالمالك — لا يعتمد فقط على اعتراض SaveChanges.
        payment.CreatedBy = userId;
        payment.CreatedAt = _clock.UtcNowOffset;
        order.InstallmentPayments.Add(payment);

        if (order.InstallmentPayments.Sum(x => x.Amount) >= order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.Completed;

        cashHolderResult.Data!.UndeliveredCashAmount += amount;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (key is not null)
        {
            // سباق إدراج بنفس المفتاح — أعد القراءة وطبّق قواعد الهوية.
            _db.Entry(payment).State = EntityState.Detached;
            _db.Entry(order).State = EntityState.Detached;
            if (cashHolderResult.Data is not null)
                _db.Entry(cashHolderResult.Data).State = EntityState.Detached;

            await tx.RollbackAsync(ct);

            await using var tx2 = await _db.Database.BeginTransactionAsync(ct);
            var raced = await _db.InstallmentPayments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClientPaymentId == key, ct);
            if (raced is null)
            {
                await tx2.RollbackAsync(ct);
                return OrderErrors.OrderNotFound;
            }

            var racedDecision = EvaluateExisting(raced, userId, orderId, amount, date);
            if (racedDecision.IsSucceeded)
                await tx2.CommitAsync(ct);
            else
                await tx2.RollbackAsync(ct);
            return racedDecision;
        }

        await _activity.AddAsync(
            order.BranchId,
            activityType,
            TargetEntityType.InstallmentPayment,
            payment.Id);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new InstallmentPaymentWriteResultDto(
            payment.Id,
            order.Id,
            payment.Amount,
            payment.Date,
            payment.ClientPaymentId,
            AlreadyApplied: false);
    }

    private static Result<InstallmentPaymentWriteResultDto> EvaluateExisting(
        InstallmentPayment existing,
        int userId,
        int orderId,
        double amount,
        DateOnly date)
    {
        // حساب آخر: لا نجاح ولا كشف تفاصيل العملية.
        if (existing.CreatedBy != userId)
            return OrderErrors.OrderNotFound;

        if (existing.OrderId != orderId
            || !AmountsEqual(existing.Amount, amount)
            || existing.Date != date)
        {
            return OrderErrors.ClientPaymentPayloadConflict;
        }

        return new InstallmentPaymentWriteResultDto(
            existing.Id,
            existing.OrderId,
            existing.Amount,
            existing.Date,
            existing.ClientPaymentId,
            AlreadyApplied: true);
    }

    private static bool AmountsEqual(double a, double b) =>
        Math.Abs(a - b) < 0.005;

    // namespaces ثنائية (int,int) — منفصلة عن ensure-draft=791901 وعن قفل الإرسال أحادي المفتاح.
    // الترتيب الإلزامي لتجنّب deadlock: الطلب أولاً ثم حامل النقد.
    public const int OrderLockNamespace = 791902;
    public const int CashHolderLockNamespace = 791903;

    private async Task AcquireOrderLockAsync(int orderId, CancellationToken ct)
    {
        if (!IsNpgsql())
            return;

        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0}, {1})",
            [OrderLockNamespace, orderId],
            ct);
    }

    private async Task AcquireCashHolderLockAsync(int sellerId, CancellationToken ct)
    {
        if (!IsNpgsql())
            return;

        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0}, {1})",
            [CashHolderLockNamespace, sellerId],
            ct);
    }

    private bool IsNpgsql()
    {
        var provider = _db.Database.ProviderName ?? "";
        return provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    }
}
