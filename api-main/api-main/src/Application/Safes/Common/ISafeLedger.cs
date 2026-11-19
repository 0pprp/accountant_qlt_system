using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;

namespace Application.Safes.Common;

/// <summary>
/// طلب تسجيل حركة يقودها النظام. <see cref="Amount"/> مقدار موجب دائماً —
/// الإشارة تُشتق من <see cref="Kind"/> داخل الـledger.
/// </summary>
public sealed record SafeLedgerEntry
{
    public required int SafeId { get; init; }
    public required SafeMovementKind Kind { get; init; }
    public required double Amount { get; init; }
    public string? Note { get; init; }
    public int? PerformedByUserId { get; init; }
    public int? InstallmentPaymentId { get; init; }
    public int? ExpenseId { get; init; }
    public int? PurchaseId { get; init; }
    public int? OrderId { get; init; }
}

/// <summary>
/// نقطة العبور الوحيدة لكل تغيير على رصيد خزينة. لا يُعدَّل <c>Safe.Balance</c> من أي مكان آخر،
/// فيبقى الرصيد مساوياً لمجموع حركات <see cref="SafeMovement"/> دائماً.
/// </summary>
public interface ISafeLedger
{
    /// <summary>إضافة نقد — مبلغ وملاحظة.</summary>
    Task<Result<SafeMovement>> DepositAsync(
        int safeId, double amount, string? note, Guid? operationKey, CancellationToken cancellationToken);

    /// <summary>سحب نقد — ينقل المبلغ إلى سجل صرفيات الفرع فلا يضيع من النظام.</summary>
    Task<Result<SafeMovement>> WithdrawAsync(
        int safeId, double amount, string? note, Guid? operationKey, CancellationToken cancellationToken);

    /// <summary>نقل بين خزينتين في نفس الفرع — يكتب صفّين مرتبطين بـ TransferGroupId.</summary>
    Task<Result<SafeMovement>> TransferAsync(
        int sourceSafeId, int destinationSafeId, double amount, string? note, Guid? operationKey,
        CancellationToken cancellationToken);

    /// <summary>حركة يقودها النظام: تحصيل، مقدمة، مصروف، مشترى، إرجاع.</summary>
    Task<Result<SafeMovement>> RecordAsync(SafeLedgerEntry entry, CancellationToken cancellationToken);

    /// <summary>رصيد افتتاحي مُرحَّل — يقبل مبلغاً موقّعاً.</summary>
    Task<Result<SafeMovement>> ApplyOpeningAsync(
        int safeId, double signedAmount, string? note, int performedByUserId, CancellationToken cancellationToken);

    Task<Result<Domain.Entities.SafeAggregate.Safe>> GetBranchSafeAsync(
        int branchId, CancellationToken cancellationToken);

    Task<Result<Domain.Entities.SafeAggregate.Safe>> GetExpensesSafeAsync(
        int branchId, CancellationToken cancellationToken);

    /// <summary>
    /// خزينة التحصيل: المتابع المُحصِّل تدخل خزينته الشخصية، والمندوب تدخل خزينة القائمة.
    /// </summary>
    Task<Result<Domain.Entities.SafeAggregate.Safe>> ResolveCollectionSafeAsync(
        int orderListId, int collectorUserId, CancellationToken cancellationToken);
}
