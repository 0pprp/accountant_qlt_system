namespace Application.Orders.Queries.Admin.GetInstallmentPayments;

public record GetOrderInstallmentPaymentsResponse
{
    public int? Id { get; set; }
    public DateOnly Date { get; set; }
    public double? Amount { get; set; }
    public string? Description { get; set; }
    public bool HasPayment { get; set; }
}