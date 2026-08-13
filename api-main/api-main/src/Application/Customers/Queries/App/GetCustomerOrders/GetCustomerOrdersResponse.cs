namespace Application.Customers.Queries.App.GetCustomerOrders;

public record GetCustomerOrdersResponse
{
    public int Id { get; set; }
    public double SellAmount { get; set; }
    public double PaidAmount { get; set; }
    public double OverdueAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}