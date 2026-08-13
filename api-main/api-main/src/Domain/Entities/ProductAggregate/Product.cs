using Domain.Common;
using Domain.Entities.OrderAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.WarehouseAggregate;

namespace Domain.Entities.ProductAggregate;

public class Product : AuditableEntity
{
    public Product(string name, int remainingCount, double buyAmount, double sellAmount, double dailyInstallmentAmount, string? description,
        int categoryId)
    {
        Name = name;
        RemainingCount = remainingCount;
        BuyAmount = buyAmount;
        SellAmount = sellAmount;
        DailyInstallmentAmount = dailyInstallmentAmount;
        Description = description;
        CategoryId = categoryId;
        OrderItems = new HashSet<OrderItem>();
        PurchaseItems = new HashSet<PurchaseItem>();
    }

    public string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? WarehouseId { get; set; }

    public ProductCategory? Category { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? Creator { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<PurchaseItem> PurchaseItems { get; set; }

    public void Update(string name, int remainingCount, double buyAmount, double sellAmount, double dailyInstallmentAmount,
        string? description, int categoryId, int warehouseId)
    {
        Name = name;
        RemainingCount = remainingCount;
        BuyAmount = buyAmount;
        SellAmount = sellAmount;
        DailyInstallmentAmount = dailyInstallmentAmount;
        Description = description;
        CategoryId = categoryId;
        WarehouseId = warehouseId;
    }
}