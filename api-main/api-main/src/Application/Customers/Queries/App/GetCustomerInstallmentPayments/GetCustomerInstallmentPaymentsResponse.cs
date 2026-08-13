namespace Application.Customers.Queries.App.GetCustomerInstallmentPayments;

public record GetCustomerInstallmentPaymentsResponse
{
    public int Id { get; set; }
    public required string CustomerFullName { get; set; }
    public double Amount { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public int OrderId { get; set; }
}