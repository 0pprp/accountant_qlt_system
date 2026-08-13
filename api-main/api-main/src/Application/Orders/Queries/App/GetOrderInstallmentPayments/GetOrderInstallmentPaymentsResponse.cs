namespace Application.Orders.Queries.App.GetOrderInstallmentPayments;

public record GetOrderInstallmentPaymentsResponse
{
    public int Id { get; set; }
    public required string CustomerFullName { get; set; }
    public double SellAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required List<InstallmentPaymentDto> InstallmentPayments { get; set; }
}

public record InstallmentPaymentDto
{
    public int? Id { get; set; }
    public DateOnly Date { get; set; }
    public double? Amount { get; set; }
    public string? Description { get; set; }
    public bool HasPayment { get; set; }
}