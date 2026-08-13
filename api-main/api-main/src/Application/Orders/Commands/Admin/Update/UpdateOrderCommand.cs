using Application.Orders.Common;

namespace Application.Orders.Commands.Admin.Update;

public record UpdateOrderCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required List<OrderItemDto> OrderItems { get; set; }
    public string? CreationAddress { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public int SellerId { get; set; }
    public int? OrderListId { get; set; }
}

public record UpdateOrderDto
{
    public required List<OrderItemDto> OrderItems { get; set; }
    public string? CreationAddress { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public int SellerId { get; set; }
    public int? OrderListId { get; set; }
}