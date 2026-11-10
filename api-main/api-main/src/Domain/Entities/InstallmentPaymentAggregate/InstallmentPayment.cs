using Domain.Common;
using Domain.Entities.OrderAggregate;

namespace Domain.Entities.InstallmentPaymentAggregate;

public class InstallmentPayment : AuditableEntity
{
    public InstallmentPayment(double amount, DateOnly date, string? description = null, Guid? clientPaymentId = null)
    {
        Amount = amount;
        Date = date;
        Description = description;
        ClientPaymentId = clientPaymentId;
    }

    public double Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    /// <summary>مفتاح إعادة الإرسال من جهاز المندوب — ثابت عبر timeout؛ يمنع القبض المكرر.</summary>
    public Guid? ClientPaymentId { get; set; }
    public int OrderId { get; set; }

    public Order? Order { get; set; }
}