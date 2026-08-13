using Domain.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;

namespace Domain.Entities.PurchaseAggregate;

public class Purchase : AuditableEntity
{
    public Purchase()
    {
    }
    
    public Purchase(int factorNumber, double totalAmount, SafeType safeType, int? branchId, Transaction transaction,
        ICollection<PurchaseItem> purchaseItems)
    {
        FactorNumber = factorNumber;
        TotalAmount = totalAmount;
        SafeType = safeType;
        BranchId = branchId;
        Transactions = [transaction];
        PurchaseItems = purchaseItems;
        Attachments = new HashSet<Attachment>();
    }

    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int? BranchId { get; set; }
    public int? OrderId { get; set; }

    public Branch? Branch { get; set; }
    public Order? Order { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<PurchaseItem> PurchaseItems { get; set; }
    public ICollection<Attachment> Attachments { get; set; }

    public void UpdateForOrder(double totalAmount, ICollection<PurchaseItem> updatedItems)
    {
        TotalAmount = totalAmount;
        foreach (var item in PurchaseItems.ToList())
            PurchaseItems.Remove(item);

        foreach (var item in updatedItems)
            PurchaseItems.Add(item);
    }
}