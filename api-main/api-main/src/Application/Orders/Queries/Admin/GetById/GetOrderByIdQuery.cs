namespace Application.Orders.Queries.Admin.GetById;

public record GetOrderByIdQuery(int Id) : IRequest<Result<GetOrderByIdResponse>>;