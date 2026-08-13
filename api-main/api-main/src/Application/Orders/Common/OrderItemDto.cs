using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Common;

public record OrderItemDto
{
    public int? Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
}

public record CreateOrderItemDto
{
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
}