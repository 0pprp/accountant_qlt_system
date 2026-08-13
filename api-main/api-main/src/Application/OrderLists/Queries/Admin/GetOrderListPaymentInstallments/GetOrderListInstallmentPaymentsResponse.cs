namespace Application.OrderLists.Queries.Admin.GetOrderListPaymentInstallments;

public record GetOrderListInstallmentPaymentsResponse
{
    public int Id { get; set; }
    public required string CustomerFullName { get; set; }
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}