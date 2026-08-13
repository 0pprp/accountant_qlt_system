namespace Application.Orders.Queries.App.GetById;

public record GetOrderByIdQuery : IRequest<Result<GetOrderByIdResponse>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}