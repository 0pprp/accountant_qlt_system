using Domain.Common;
using Domain.Entities.BranchAggregate;

namespace Domain.Entities.SafeAggregate;

public class Safe : AuditableEntity
{
    public Safe(string name, double remainingCashAmount, double debitAmount, double creditAmount, int? branchId)
    {
        Name = name;
        RemainingCashAmount = remainingCashAmount;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
        BranchId = branchId;
        Transactions = new HashSet<Transaction>();
    }

    public Safe(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public double RemainingCashAmount { get; set; }
    public double DebitAmount { get; set; }
    public double CreditAmount { get; set; }
    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}