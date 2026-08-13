namespace Application.Customers.Queries.App.GetCustomerOrders;

public record GetCustomerOrdersQuery : IRequest<Result<List<GetCustomerOrdersResponse>>>
{
    public int CustomerId { get; set; }
    public int UserId { get; set; }
}