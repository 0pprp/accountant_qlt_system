using Domain.Common;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate.Enums;

namespace Domain.Entities.SafeAggregate;

/// <summary>
/// سطر مستند المصروف أو المشترى. لا يمسّ رصيد الخزينة إطلاقاً —
/// الأثر النقدي يُسجَّل في <see cref="SafeMovement"/> عبر الـledger.
/// </summary>
public class Transaction : AuditableEntity
{
    public Transaction(
        double amount,
        TransactionType type,
        TransactionStatus status,
        TransactionDirection direction,
        int safeId)
    {
        Amount = amount;
        Type = type;
        Status = status;
        Direction = direction;
        SafeId = safeId;
    }

    public double Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string? StatusDescription { get; set; }
    public TransactionDirection Direction { get; set; }
    public int SafeId { get; set; }
    public int? PurchaseId { get; set; }
    public int? ExpenseId { get; set; }

    public Safe? Safe { get; set; }
    public Purchase? Purchase { get; set; }
    public Expense? Expense { get; set; }
}
