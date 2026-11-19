using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Common;

/// <summary>
/// دخول المبيع المُتمّ إلى حزمة عشرته، وخروجه منها إن غادر «مُتمّ».
/// idempotent عبر الفهرس الفريد على <c>OrderId</c>.
///
/// لا يُستدعى داخل معاملة الـcheckout: تلك تعمل بعزل Serializable، ولو تسابق بيعان
/// لنفس الموظف على صفّ الحزمة نفسه لتراجع البيع كلّه بسبب قيد أرشيفي.
/// </summary>
public static class ArchiveIntake
{
    public const int BaghdadOffsetHours = 3;

    /// <summary>
    /// المبيع بلا بائع لا يدخل الأرشيف إطلاقاً — لا حزمة بلا موظف مبيعات.
    /// (بيانات النجف المستوردة كلها من هذا النوع.)
    /// </summary>
    public static bool IsEligible(Order order) =>
        order.WorkflowStatus == SalesWorkflowStatus.Sold
        && order.ApprovalStatus != OrderApprovalStatus.Rejected
        && order.SellerId is > 0;

    public static (DateOnly Date, ArchivePeriodSource Source) ResolvePeriodDate(Order order)
    {
        if (order.SaleDate is { } saleDate)
            return (saleDate, ArchivePeriodSource.SaleDate);

        if (order.SalesCompletedAtUtc is { } completed)
            return (ToBaghdadDate(completed), ArchivePeriodSource.SalesCompletedAt);

        var created = order.CreatedAt ?? DateTimeOffset.UtcNow;
        return (ToBaghdadDate(created), ArchivePeriodSource.OrderCreatedAt);
    }

    public static DateOnly ToBaghdadDate(DateTimeOffset moment) =>
        DateOnly.FromDateTime(moment.ToOffset(TimeSpan.FromHours(BaghdadOffsetHours)).DateTime);

    /// <summary>
    /// يُنشئ معاملة المبيع إن لم تكن موجودة. يُرجع المعاملة، أو null إن كان المبيع غير مؤهّل.
    /// لا يحفظ — المُنادي يقرّر متى يحفظ.
    /// </summary>
    public static async Task<ArchiveTransaction?> EnsureAsync(
        IApplicationDbContext db,
        Order order,
        CancellationToken cancellationToken)
    {
        if (IsEligible(order) == false)
            return null;

        var existing = await db.ArchiveTransactions
            .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);

        if (existing is not null)
        {
            existing.Revive();
            return existing;
        }

        var (date, source) = ResolvePeriodDate(order);
        var period = ArchivePeriod.For(date);
        var bundle = await EnsureBundleAsync(db, order.BranchId, order.SellerId!.Value, period, cancellationToken);

        var customer = await db.Customers.AsNoTracking()
            .Where(x => x.Id == order.CustomerId)
            .Select(x => new { x.FullName, x.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        var followerId = order.OrderListId is { } listId
            ? await db.OrderLists.AsNoTracking()
                .Where(x => x.Id == listId)
                .Select(x => (int?)x.MotabaId)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var transaction = new ArchiveTransaction(
            bundle.Id,
            order.Id,
            order.CustomerId,
            customer?.FullName ?? "—",
            customer?.PhoneNumber,
            followerId is > 0 ? followerId : null,
            date,
            source);

        db.ArchiveTransactions.Add(transaction);
        return transaction;
    }

    /// <summary>المبيع غادر «مُتمّ». الورق باقٍ عند حامله، فنُبطِل ولا نحذف.</summary>
    public static async Task<ArchiveTransaction?> VoidAsync(
        IApplicationDbContext db,
        int orderId,
        string reason,
        DateTimeOffset atUtc,
        CancellationToken cancellationToken)
    {
        var transaction = await db.ArchiveTransactions
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

        if (transaction is null)
            return null;

        return transaction.Void(reason, atUtc) ? transaction : null;
    }

    private static async Task<ArchiveBundle> EnsureBundleAsync(
        IApplicationDbContext db,
        int branchId,
        int salesEmployeeUserId,
        ArchivePeriod period,
        CancellationToken cancellationToken)
    {
        var bundle = await db.ArchiveBundles.FirstOrDefaultAsync(
            x => x.BranchId == branchId
                 && x.SalesEmployeeUserId == salesEmployeeUserId
                 && x.PeriodYear == period.Year
                 && x.PeriodMonth == period.Month
                 && x.PeriodIndex == period.Index,
            cancellationToken);

        if (bundle is not null)
            return bundle;

        var employeeName = await db.Users.AsNoTracking()
            .Where(x => x.Id == salesEmployeeUserId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        bundle = new ArchiveBundle(branchId, salesEmployeeUserId, employeeName ?? "—", period);
        db.ArchiveBundles.Add(bundle);

        // الحزمة تُحفظ فوراً لأن المعاملة تحتاج مُعرِّفها، والصفّ مستقل بذاته.
        await db.SaveChangesAsync(cancellationToken);
        return bundle;
    }
}
