namespace Application.Customers.Queries.Admin.GetById;

public record GetCustomerByIdQuery(int Id) : IRequest<Result<GetCustomerByIdResponse>>;