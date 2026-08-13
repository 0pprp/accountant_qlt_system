using Application.Orders.Common;

namespace Application.Customers.Queries.Admin.GetOrders;

public record GetCustomerOrdersResponse
{
    public int Id { get; set; }
    public double SellAmount { get; set; }
    public required string SellerFullName { get; set; }
    public required string OrderListName { get; set; }
    public required List<OrderItemDto> OrderItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}