using Domain.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;

namespace Domain.Entities.ExpenseAggregate;

public class Expense : AuditableEntity
{
    public Expense()
    {
    }
    
    public Expense(int factorNumber, double totalAmount, SafeType safeType, int? branchId, Transaction transaction,
        ICollection<ExpenseItem> expenseItems)
    {
        FactorNumber = factorNumber;
        TotalAmount = totalAmount;
        SafeType = safeType;
        BranchId = branchId;
        Transactions = [transaction];
        ExpenseItems = expenseItems;
        Attachments = new HashSet<Attachment>();
    }

    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<ExpenseItem> ExpenseItems { get; set; }
    public ICollection<Attachment> Attachments { get; set; }
}