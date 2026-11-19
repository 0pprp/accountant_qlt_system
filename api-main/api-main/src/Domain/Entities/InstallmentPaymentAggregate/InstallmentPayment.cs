using Domain.Common;
using Domain.Entities.CustomerAggregate;
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

    /// <summary>مفتاح استيراد ثابت — مثال: najaf|CustomersPayments|{id}.</summary>
    public string? ImportKey { get; set; }

    /// <summary>
    /// المبيع المرتبط عند التخصيص. اختياري للترحيل التاريخي عندما يكون الوصل على مستوى الزبون
    /// في المصدر (CustomersPayments / AddToBox) دون ربط مبيع واحد.
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>الزبون صاحب الوصل عند غياب/تعدد التخصيص للمبيع.</summary>
    public int? CustomerId { get; set; }

    public Order? Order { get; set; }
    public Customer? Customer { get; set; }
}