using Application.Orders.Common;
using Domain.Entities.OrderAggregate;

namespace Application.Orders.Commands.App.Update;

public record UpdateOrderCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required List<OrderItemDto> OrderItems { get; set; }
    public string? CreationAddress { get; set; }
    public Location? Location { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public int? OrderListId { get; set; }
    public int UserId { get; set; }
}

public record UpdateOrderDto
{
    public required List<OrderItemDto> OrderItems { get; set; }
    public string? CreationAddress { get; set; }
    public Location? Location { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public int? OrderListId { get; set; }
}