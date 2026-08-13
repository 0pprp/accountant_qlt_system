namespace Application.Customers.Queries.Admin.GetOrders;

public record GetCustomerOrdersQuery(int CustomerId) : IRequest<Result<List<GetCustomerOrdersResponse>>>;