using Application.Orders.Common;

namespace Application.Orders.Commands.App.Create;

public record CreateOrderCommand : IRequest<Result<CreateOrderResponse>>
{
    public required List<CreateOrderItemDto> OrderItems { get; set; }
    public int CustomerId { get; set; }
    public int UserId { get; set; }
}

public record CreateOrderDto
{
    public required List<CreateOrderItemDto> OrderItems { get; set; }
    public int CustomerId { get; set; }
}