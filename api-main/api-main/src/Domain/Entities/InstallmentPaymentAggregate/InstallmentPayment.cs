using Domain.Common;
using Domain.Entities.OrderAggregate;

namespace Domain.Entities.InstallmentPaymentAggregate;

public class InstallmentPayment : AuditableEntity
{
    public InstallmentPayment(double amount, DateOnly date, string? description = null)
    {
        Amount = amount;
        Date = date;
    }

    public double Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public int OrderId { get; set; }

    public Order? Order { get; set; }
}