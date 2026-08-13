namespace Application.Orders.Commands.App.Delete;

public record DeleteOrderCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}