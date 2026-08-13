namespace Application.Customers.Queries.App.GetById;

public record GetCustomerByIdQuery(int Id) : IRequest<Result<GetCustomerByIdResponse>>;