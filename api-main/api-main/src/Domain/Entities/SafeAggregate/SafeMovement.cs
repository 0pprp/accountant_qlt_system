using Domain.Common;
using Domain.Entities.SafeAggregate.Enums;

namespace Domain.Entities.SafeAggregate;

/// <summary>
/// حركة على خزينة — سجل غير قابل للتعديل وهو مصدر الحقيقة للرصيد.
/// <para>
/// <see cref="Amount"/> موقّع: موجب يعني دخول نقد وسالب يعني خروجه، فيتحقق الرصيد بـ
/// <c>SUM(Amount) == Safe.Balance</c>. الإشارة تُشتق في المصانع أدناه فلا يضبطها المُنادي يدوياً.
/// </para>
/// </summary>
public class SafeMovement : AuditableEntity
{
    public const int NoteMaxLength = 500;
    public const int ImportKeyMaxLength = 120;

    private SafeMovement()
    {
    }

    private SafeMovement(
        int safeId,
        SafeMovementKind kind,
        double amount,
        double balanceAfter,
        string? note,
        int performedByUserId)
    {
        SafeId = safeId;
        Kind = kind;
        Amount = amount;
        BalanceAfter = balanceAfter;
        Note = Normalize(note);
        PerformedByUserId = performedByUserId;
    }

    public int SafeId { get; private set; }
    public SafeMovementKind Kind { get; private set; }

    /// <summary>المبلغ الموقّع — موجب داخل، سالب خارج.</summary>
    public double Amount { get; private set; }

    /// <summary>لقطة الرصيد بعد تطبيق الحركة — تكشف أي تلاعب لاحق.</summary>
    public double BalanceAfter { get; private set; }

    public string? Note { get; private set; }

    /// <summary>منفّذ العملية صريحاً، مستقلاً عن <c>CreatedBy</c> الذي يضبطه الـinterceptor.</summary>
    public int PerformedByUserId { get; private set; }

    /// <summary>الخزينة المقابلة في النقل والسحب.</summary>
    public int? CounterpartySafeId { get; private set; }

    /// <summary>يربط صفّي النقل (خارج + داخل) ببعضهما.</summary>
    public Guid? TransferGroupId { get; private set; }

    /// <summary>مفتاح منع التكرار من العميل — نقرة مزدوجة أو إعادة إرسال لا تُنتج حركة ثانية.</summary>
    public Guid? OperationKey { get; private set; }

    public int? InstallmentPaymentId { get; private set; }
    public int? ExpenseId { get; private set; }
    public int? PurchaseId { get; private set; }
    public int? OrderId { get; private set; }

    /// <summary>مفتاح استيراد ثابت للحركات التاريخية — مثال: <c>najaf|WithdrawalFromBox|{id}</c>.</summary>
    public string? ImportKey { get; private set; }

    public Safe? Safe { get; set; }
    public Safe? CounterpartySafe { get; set; }

    /// <summary>رصيد افتتاحي مُرحَّل — يقبل مبلغاً موقّعاً لأن الرصيد المُرحَّل قد يكون سالباً.</summary>
    public static SafeMovement Opening(
        int safeId, double signedAmount, double balanceAfter, string? note, int performedByUserId) =>
        new(safeId, SafeMovementKind.Opening, signedAmount, balanceAfter, note, performedByUserId);

    public static SafeMovement Deposit(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId, Guid? operationKey) =>
        new(safeId, SafeMovementKind.Deposit, Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            OperationKey = operationKey
        };

    /// <summary>خروج نهائي من الخزينة بلا طرف مقابل — النقل بين الخزائن له TransferOut.</summary>
    public static SafeMovement Withdrawal(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        Guid? operationKey) =>
        new(safeId, SafeMovementKind.Withdrawal, -Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            OperationKey = operationKey
        };

    public static SafeMovement TransferOut(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        Guid? operationKey, int destinationSafeId, Guid transferGroupId) =>
        new(safeId, SafeMovementKind.TransferOut, -Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            OperationKey = operationKey,
            CounterpartySafeId = destinationSafeId,
            TransferGroupId = transferGroupId
        };

    public static SafeMovement TransferIn(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        int sourceSafeId, Guid transferGroupId) =>
        new(safeId, SafeMovementKind.TransferIn, Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            CounterpartySafeId = sourceSafeId,
            TransferGroupId = transferGroupId
        };

    /// <summary>تحصيل قسط — يدخل خزينة القائمة أو خزينة المتابع المُحصِّل.</summary>
    public static SafeMovement Collection(
        int safeId, double amount, double balanceAfter, int performedByUserId,
        int installmentPaymentId, int orderId, string? note = null) =>
        new(safeId, SafeMovementKind.Collection, Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            InstallmentPaymentId = installmentPaymentId,
            OrderId = orderId
        };

    /// <summary>عكس تحصيل قسط عند حذف التسديد أو تخفيض مبلغه — خروج مربوط بنفس التسديد.</summary>
    public static SafeMovement CollectionReversal(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        int installmentPaymentId, int? orderId) =>
        new(safeId, SafeMovementKind.Withdrawal, -Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            InstallmentPaymentId = installmentPaymentId,
            OrderId = orderId
        };

    /// <summary>مقدمة بيع — تدخل خزينة الموظف الذي استلمها.</summary>
    public static SafeMovement Prepayment(
        int safeId, double amount, double balanceAfter, int performedByUserId, int orderId) =>
        new(safeId, SafeMovementKind.Prepayment, Math.Abs(amount), balanceAfter, null, performedByUserId)
        {
            OrderId = orderId
        };

    public static SafeMovement ExpenseOut(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId, int expenseId) =>
        new(safeId, SafeMovementKind.ExpenseOut, -Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            ExpenseId = expenseId
        };

    public static SafeMovement PurchaseOut(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        int? purchaseId, int? orderId) =>
        new(safeId, SafeMovementKind.PurchaseOut, -Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            PurchaseId = purchaseId,
            OrderId = orderId
        };

    /// <summary>إرجاع نقد للخزينة عند تخفيض مصروف أو مشترى.</summary>
    public static SafeMovement Refund(
        int safeId, double amount, double balanceAfter, string? note, int performedByUserId,
        int? expenseId, int? purchaseId, int? orderId) =>
        new(safeId, SafeMovementKind.Refund, Math.Abs(amount), balanceAfter, note, performedByUserId)
        {
            ExpenseId = expenseId,
            PurchaseId = purchaseId,
            OrderId = orderId
        };

    /// <summary>حركة تاريخية مستوردة — المبلغ موقّع كما في المصدر.</summary>
    public static SafeMovement Imported(
        int safeId, SafeMovementKind kind, double signedAmount, double balanceAfter,
        string? note, int performedByUserId, string importKey) =>
        new(safeId, kind, signedAmount, balanceAfter, note, performedByUserId)
        {
            ImportKey = importKey
        };

    private static string? Normalize(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        var trimmed = note.Trim();
        return trimmed.Length <= NoteMaxLength ? trimmed : trimmed[..NoteMaxLength];
    }
}
