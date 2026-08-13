using Domain.Common;
using Domain.Entities.ProductAggregate;

namespace Domain.Entities.PurchaseAggregate;

public class PurchaseItem : AuditableEntity
{

    public PurchaseItem(int quantity, double amount, int productId)
    {
        Quantity = quantity;
        Amount = amount;
        ForeignProductName = null;
        ProductId = productId;
    }

    public PurchaseItem(int quantity, double amount, string? foreignProductName)
    {
        Quantity = quantity;
        Amount = amount;
        ForeignProductName = foreignProductName;
        ProductId = null;
    }

    public int Quantity { get; set; }
    public double Amount { get; set; }
    public string? ForeignProductName { get; set; }
    public int? ProductId { get; set; }
    public int PurchaseId { get; set; }

    public Product? Product { get; set; }
    public Purchase? Purchase { get; set; }
}