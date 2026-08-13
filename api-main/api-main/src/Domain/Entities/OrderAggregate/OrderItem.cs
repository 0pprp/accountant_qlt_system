using Domain.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.ProductAggregate;

namespace Domain.Entities.OrderAggregate;

public class OrderItem : AuditableEntity
{
    public OrderItem(
        ProductType productType,
        int quantity,
        double buyAmount,
        double sellAmount,
        double prepaymentAmount,
        double dailyInstallmentAmount,
        string? productName,
        int? productId)
    {
        ProductType = productType;
        Quantity = quantity;
        BuyAmount = buyAmount;
        SellAmount = sellAmount;
        PrepaymentAmount = prepaymentAmount;
        DailyInstallmentAmount = dailyInstallmentAmount;
        ProductName = productName;
        ProductId = productId;
    }

    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? ProductName { get; set; }
    public int? ProductId { get; set; }
    public int OrderId { get; set; }

    public Product? Product { get; set; }
    public Order? Order { get; set; }
}